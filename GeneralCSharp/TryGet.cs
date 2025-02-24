namespace CodingPatterns.GeneralCSharp
{
    public class TryGet
    {
        //Example on how to use it with a dictionary
        
        public Dictionary<string, int> Example = new Dictionary<string, int>()
        {
            {"A", 10},
            {"B", 20},
            {"C", 30}
        };
        
        //What you would write
        public void PrintNumberOld(string example = "A")
        {
            if (Example.ContainsKey(example))
            {
                var number = Example[example];
                Console.WriteLine($"The number for {example} is {number}");
            } else Console.WriteLine($"The example {example} was not found in the dictionary.");
        }
        
        //What you should write
        public void PrintNumber(string example = "A")
        {
            if (Example.TryGetValue(example, out var number)) Console.WriteLine($"The number for {example} is {number}");
            else Console.WriteLine($"The example {example} was not found in the dictionary.");
        }
        
        //Ok, but why would i use this outside of a dictionary?
        
        //An example of code in a hypothetical game server implementation
        //Imagine if there was a database containing all users that play on the server
        public UserDataBase UserDataBase { get; } = new UserDataBase();
        
        //Now we can implement methods in a much more elegant way
        public void Kick(string username, string reason)
        {
            if (!UserDataBase.TryGetUser(username, out var user)) return;
            user.Disconnect();
            Console.WriteLine($"Kicked {username} for {reason}.");
        }

        //When combined with a return statement, TryGet can be a very powerful pattern
        public void RunAgeCheck()
        {
            if (!Settings.TryGetSetting("AgeCheck", out bool shouldCheck) || !shouldCheck) return;
            if (!UserDataBase.TryGetUsers(18, out var users)) return;
            foreach (var user in users) Kick(user.Name, "You are not old enough to play.");
        }
        
    }

    //Ok But how would I implement functions like this?
    
    // __________________________________________
    // |                                        |
    // |    Implementation Examples             |
    // |                                        |
    // ------------------------------------------
 
    //Let's say we have a user
    public class User
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public void Disconnect() { }
    }

    // We can implement our database like this    
    public class UserDataBase
    {
        public User[] Users { get; set; } //Let's ignore optimization for now and keep it an array

        //We can implement the TryGetUser function as follows (using Linq (slow but this is just an example))
        public bool TryGetUser(string username, out User user)
        {
            user = Users.FirstOrDefault(x => x.Name.Equals(username, StringComparison.Ordinal));
            return user != null;
        }

        //Or we can add a function to query the database for a parameter like this and return true if multiple results are found
        public bool TryGetUsers(int age, out User[] users)
        {
            users = Users.Where(x => x.Age >= age).ToArray();
            return users.Length > 0;
        }
    }

    //So we use this to replace one null check?
    // Well mostly yes, but the pattern can be used in different ways as well, you just have to get creative
    // An example where the pattern is very useful is combined with type checking, or when you need multiple if statements after a lookup
    // For example lets say we have a settings class like this
    
    public static class Settings
    {
        // We have one memory location where all settings are deserialized too regardless of type or name. This way it is modular and expandable in the future
        private static Dictionary<string, object> ServerSettings { get; set; } = new Dictionary<string, object>()
        {
            { "AgeCheck", true },
            { "Port", 8080 },
            { "Logs", false },
        };

        //Now what if we want to get a setting from the list. We expect it to be a bool
        //In the serialization world, i have noticed that a normal way to tell the function caller about a success or failure is an exception
        //But you could also handle it like this
        public static bool TryGetSetting(string name, out bool value)
        {
            value = default;
            if (!ServerSettings.TryGetValue(name, out var objValue)) return false; //check if the setting exists
            if (objValue is not bool boolValue) return false; //check if it has the correct type
            value = boolValue;
            return true;
        }
        
        //This way you can set up multiple function overloads for different types, and the caller will almost 100% implement edge cases for you
        public static bool TryGetSetting(string name, out int value)
        {
            value = default;
            if (!ServerSettings.TryGetValue(name, out var objValue)) return false;
            if (objValue is not int intValue) return false;
            value = intValue;
            return true;
        }
        
        //But what about cases when the error type is relevant, maybe the caller wants to know why a thing is false?
        //Well then you can use an Enum out parameter to indicate the error type
        public enum ExampleSettingError
        {
            Success,
            SettingNotFound,
            SettingOfDifferentType
        }

        public static bool TryGetSettingWithError(string name, out float value, out ExampleSettingError error)
        {
            error = ExampleSettingError.Success;
            value = default;
            if (!ServerSettings.TryGetValue(name, out var objValue))
            {
                error = ExampleSettingError.SettingNotFound;
                return false;
            }
            if (objValue is not float floatValue)
            {
                error = ExampleSettingError.SettingOfDifferentType;
                return false;
            }
            value = floatValue;
            return true;
        }
        
        //Now the caller can do the following
        public static void SomeCodeCallingTheFunction()
        {
            if (!TryGetSettingWithError("Test", out float value, out var error))
            {
                switch (error)
                {
                    case ExampleSettingError.Success: break; //Should never happen unless the function was not implemented properly
                    case ExampleSettingError.SettingNotFound: Console.Error.WriteLine("The setting did not exist"); break;
                    case ExampleSettingError.SettingOfDifferentType: Console.Error.WriteLine("The setting was not a float"); break;
                    default: throw new Exception($"The API was updated without me knowing! The error: {error.ToString()}");
                }
                return;
            }
            Console.WriteLine($"The setting value was {value}");
        }
        
        //Due to the precompiled nature of enums, the caller can be 100% sure that all edge cases are implemented
        //If they wanted too they could even add a default case for when you update your api in the future
    }

}