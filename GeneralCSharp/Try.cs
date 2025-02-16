using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace CodingPatterns.GeneralCSharp
{
    public class Try
    {
        public Dictionary<string, int> Example = new Dictionary<string, int>()
        {
            {"A", 10},
            {"B", 20},
            {"C", 30}
        };
        
        public UserDataBase UserDataBase { get; } = new UserDataBase();
        
        public void WithDictionary(string example = "A")
        {
            if (!Example.TryGetValue(example, out var number)) return;
            
            Console.WriteLine($"The number for {example} is {number}");
        }

        public void Kick(string username, string reason)
        {
            if (!UserDataBase.TryGetUser(username, out var user)) return;
            user.Disconnect();
            Console.WriteLine($"Kicked {username} for {reason}.");
        }

        public void RunAgeCheck()
        {
            if (!Settings.TryGetSetting("AgeCheck", out bool shouldCheck) || !shouldCheck) return;
            if (!UserDataBase.TryGetUsers(18, out var users)) return;
            foreach (var user in users) Kick(user.Name, "You are not old enough to play.");
        }
        
    }

    public static class Settings
    {
        private static Dictionary<string, object> ServerSettings { get; set; } = new Dictionary<string, object>()
        {
            { "AgeCheck", true },
            { "Port", 8080 },
            { "Logs", false },
        };

        public static bool TryGetSetting(string name, out bool value)
        {
            value = false;
            if (!ServerSettings.TryGetValue(name, out var objValue)) return false;
            if (objValue is not bool boolValue) return false;
            value = boolValue;
            return true;
        }
        
        public static bool TryGetSetting(string name, out int value)
        {
            value = 0;
            if (!ServerSettings.TryGetValue(name, out var objValue)) return false;
            if (objValue is not int intValue) return false;
            value = intValue;
            return true;
        }
    }

    public class UserDataBase
    {
        public User[] Users { get; set; }

        public bool TryGetUser(string username, out User user)
        {
            user = Users.FirstOrDefault(x => x.Name.Equals(username, StringComparison.Ordinal));
            return user != null;
        }

        public bool TryGetUsers(int age, out User[] users)
        {
            users = Users.Where(x => x.Age >= age).ToArray();
            return users.Length > 0;
        }
    }

    public class User
    {
        public string Name { get; set; }
        public int Age { get; set; }

        public void Disconnect() { }
    }
}