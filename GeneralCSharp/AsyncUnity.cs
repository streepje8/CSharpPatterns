using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodingPatterns.GeneralCSharp;

public class AsyncUnity : MonoBehaviour
{
    public delegate Task AsyncFunction();

    public void CallAsynchronously(AsyncFunction function)
    {
        function().ContinueWith(t =>
        {
            //This will cause all errors to be mirrored in the unity console
            if (t.IsFaulted) Debug.LogException(t.Exception); //Collect errors, otherwise they disappear into a deep void when the function finishes
            
            //If you want to do it fully properly you should do the following
            //This is because the ContinueWith function might be called on a different thread, and officially Debug LogException is not thread safe
            var capture = ExceptionDispatchInfo.Capture(t.Exception?.InnerException ?? t.Exception ?? new Exception("Exception was null"));
            MainThreadTasks.Add(() => capture.Throw());
        });
    }
    
    //But how do we make our calls thread safe?
    //The easiest way is to have a mono behaviour to sync the calls
    
    //Because unity is not thread safe, we have to run certain code on the main thread
    //The easiest way I found to do this is by having an update function running all the code in a try catch
    private delegate void MainTreadTask();
    private static ConcurrentBag<MainTreadTask> MainThreadTasks = new ConcurrentBag<MainTreadTask>(); //This is a thread safe list
    void Update()
    {
        //Execute all queued tasks
        while(!MainThreadTasks.IsEmpty)
        {
            if (!MainThreadTasks.TryTake(out var task)) continue;
            try { task(); } catch(Exception e) { Debug.LogException(e); }
        }
    }
    
    //Ok so we know how to call functions async, and how to sync things with the main thread
    //Now we can write async functions without blocking the main thread
    public async Task MyAsyncCode()
    {
        var data = await File.ReadAllBytesAsync(@"X:\Some\File\Path");
        
        Rigidbody rb = null;
        await MainThreadCall(() => rb = new GameObject().AddComponent<Rigidbody>()); //We wait for the main thread before we continue
        
        if (rb is not null)
        {
            //You can do things in a async context here
            var newPosition = rb.position + Vector3.one;
            MainThreadTasks.Add(() => rb.Move(newPosition, Quaternion.identity)); //Code execution will continue immediately after this method call, instead of waiting for the main thread
        }
        
        //Async code like this can be useful when loading scenes or doing things like that
        var sceneLoad = SceneManager.LoadSceneAsync(1);
        while (!sceneLoad.isDone) await Task.Delay(25);
        MainThreadTasks.Add(() => Debug.Log("Scene finished loading"));
    }

    //If you are wondering how we await a main thread call, I usually implement it like this
    public delegate void MainThreadTaskCall();
    public async Task MainThreadCall(MainThreadTaskCall call, int pollingRateMs = 25)
    {
        var canContinue = false;
        MainThreadTasks.Add(() =>
        {
            try {call();} finally { canContinue = true;}
        });
        while (!canContinue) await Task.Delay(pollingRateMs);
    }
    
    //You can also use this to make external library calls run in the background instead of blocking the main thread
    //This example allows the opensource project Standalone File Browser to run without blocking your main thread
    //It also properly transfers the exceptions from one thread to another if they occur
    public static async Task<string> SavePanelAsync(string title, string directory, string defaultName, string extension)
    {
        var path = string.Empty;
        var complete = false;
        var success = true;
        ExceptionDispatchInfo exception = null;
        new Thread(() =>
        {
            try { path = StandaloneFileBrowser.SaveFilePanel(title, directory, defaultName, extension); }
            catch (Exception e) { exception = ExceptionDispatchInfo.Capture(e); success = false; }
            complete = true;
        }).Start();
        while (!complete) await Task.Delay(25);
        if (!success) exception.Throw();
        return path;
    }
}

//https://github.com/gkngkc/UnityStandaloneFileBrowser
public class StandaloneFileBrowser { public static string SaveFilePanel(string title, string directory, string defaultName, string extension) => ""; }