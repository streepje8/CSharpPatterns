using System.Collections;
using UnityEngine;

namespace CodingPatterns.GeneralCSharp;

public class TupleDeconstruction
{
    
    //Tuple Deconstruction can be used to quickly batch a collection of data together without making a new class
    //The details will be combined into a passed by value struct by the system for you
    public void PrintCustomerDetails()
    {
        //When returning, you can unpack the variables inline like this
        var (name, address, zip) = GetCustomerDetails();
        Debug.Log($"Details: [{name}, {address}, {zip}]");
        
        //Or only keep select ones
        var (nameOnly, _, _) = GetCustomerDetails();
        Debug.Log($"Name: {nameOnly}");
    }

    //The function that retuns the values looks like this
    private (string name, string adrress, string zip) GetCustomerDetails()
    {
        //And the values can be packed easily inline
        return ("Pardoes", "Europalaan 1", "5171KW");
    }

    //You can also combine this with IEnumerable's like dictionaries or lists to create inline datastructures to quickly hack prototypes together
    public static Scoreboard Scores { get; } = new();
    public void PrintScores()
    {
        foreach (var (name, time, score) in Scores)
        {
            Debug.Log($"[{name}] score: {score}, scored at: {time:d}");
        }
    }
}

public class Scoreboard : IEnumerable<(string, DateTime, int)> 
{
    public IEnumerator<(string, DateTime, int)> GetScores()
    {
        yield return ("AAA", DateTime.Now, 428);
        yield return ("AAB", DateTime.Now + TimeSpan.FromDays(2), 230);
        yield return ("ABC", DateTime.Now - TimeSpan.FromDays(4), 618);
        yield return ("ABB", DateTime.Now + TimeSpan.FromDays(7), 298);
    }

    public IEnumerator<(string, DateTime, int)> GetEnumerator()
    {
        return GetScores();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}