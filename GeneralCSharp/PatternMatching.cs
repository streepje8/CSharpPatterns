using UnityEngine;

namespace CodingPatterns.GeneralCSharp;

public class PatternMatching
{
    public void OnAType(Component comp)
    {
        //Quickly check and cast
        if (comp is MeshRenderer renderer)
        {
            //Do something with renderer
            renderer.material.color = Color.blue;
        }

        //Switch on type
        switch (comp)
        {
            case BoxCollider box:
                break;
            case SphereCollider sphere:
                break;
            case CapsuleCollider capsule:
                break;
        }

        //Or combine it with a return
        if (comp is not AudioSource source) return;
        source.Play();
    }

    public enum State
    {
        Invalid,
        Solid,
        Gas,
        Liquid
    }

    public static State GetWaterState(int temperature)
    {
        return temperature switch
        {
            <= 0 => State.Solid,
            > 0 and < 100 => State.Liquid,
            > 100 => State.Gas,
            _ => State.Invalid
        };
    }

    //Parse files
    //Very naive implementation which handles 0 edge cases and exceptions, but this is just an example.
    
    public static ObjMesh ParseObj(string filePath) 
    {
        var mesh = new ObjMesh();
        bool success = true;
        foreach (var line in File.ReadLines(filePath))
        {
            success &= line.Split(" ") switch
            {
                ["v", var x, var y, var z] => mesh.ParseAppendVert(x,y,z),
                ["vt", var u, var v] => mesh.ParseAppendUv(u,v),
                ["vn", var x, var y, var z] => mesh.ParseAppendNormal(x,y,z),
                ["f", var a, var b, var c] => mesh.ParseAppendFace(a,b,c),
                _ => false
            };
        }
        if(!success) throw new Exception("Parsing failed!");
        return mesh;
    }
}

public class ObjMesh
{
    public bool ParseAppendVert(string x, string y, string z) => false;
    public bool ParseAppendUv(string u, string v) => false;
    public bool ParseAppendNormal(string x, string y, string z) => false;
    public bool ParseAppendFace(string a, string b, string c) => false;
}