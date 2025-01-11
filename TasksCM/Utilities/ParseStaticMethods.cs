using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Reactive.Linq;

namespace TasksCM.Utilities;


/// <summary>
/// This class contains static methods for using Parse in the application.
/// You'll see how ridiculously easy it is to use Parse in your application.
/// Each method is a static method that you can call from anywhere in your application.
/// You just need to be good at handling Async Programming though (You become the Conductor! )
/// </summary>
public static class ParseStaticMethods
{

    public static bool InitializeParseClient()
    {
        try
        {
            // Check for internet connection
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                Debug.WriteLine("No Internet Connection: Unable to initialize ParseClient.");
                return false;
            }

            // Validate API Keys
            if (string.IsNullOrEmpty(APIKeys.ApplicationId) ||
                string.IsNullOrEmpty(APIKeys.ServerUri) ||
                string.IsNullOrEmpty(APIKeys.DotNetKEY))
            {
                Debug.WriteLine("Invalid API Keys: Unable to initialize ParseClient.");
                return false;
            }

            // Create ParseClient
            ParseClient client = new ParseClient(new ServerConnectionData
            {
                ApplicationID = APIKeys.ApplicationId,
                ServerURI = APIKeys.ServerUri,
                Key = APIKeys.DotNetKEY,
            }
            );
            client.Publicize();
            Debug.WriteLine("ParseClient initialized successfully.");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error initializing ParseClient: {ex.Message}");
            return false;
        }
    }

    private static void RestoreAllData(object sender, EventArgs e)
    {
        //will be helpful when you want to restore all data from a backup

        //var Links = await ParseClient.Instance.CallCloudCodeFunctionAsync<string>("restoreAllData", dataToRestore);

    }


    private static void OnLogOut(object sender, EventArgs e)
    {
        ParseClient.Instance.LogOut();
    }

    public static async Task<string> ShareTask(string taskId)
    {
        try
        {
            var result = await ParseClient.Instance.CallCloudCodeFunctionAsync<string>(
                "generateShareCode",
                new Dictionary<string, object> { { "taskId", taskId } }
            );

            Console.WriteLine($"Generated Share Code: {result}");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sharing task: {ex.Message}");
            return null;
        }
    }

    public static async Task<TaskObject> AcceptSharedTask(string shareCode)
    {
        try
        {
            var result = await ParseClient.Instance.CallCloudCodeFunctionAsync<Dictionary<string, object>>(
                "acceptSharedTask",
                new Dictionary<string, object> { { "shareCode", shareCode } }
            );

            if (result.TryGetValue("isReadOnly", out object? value) && (bool)value)
            {
                Console.WriteLine("Task is shared in read-only mode.");
            }

            var taskData = (ParseObject)result["task"];
            

            //Console.WriteLine($"Task '{taskData.Name}' accepted successfully.");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error accepting shared task: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// This method logins a user with the provided username and password.
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public static async Task<ParseUser?> LoginUser(string username, string password)
    {

        var usr = await ParseClient.Instance.LogInWithAsync(username, password);


        if (usr is null)
        {
            return null;
        }

        return usr;
    }

    public static async Task SignUpUser(string username, string password, string email)
    {
        var user = new ParseUser
        {
            Username = username,
            Password = password,
            Email = email
        };
        await ParseClient.Instance.SignUpWithAsync(user);
        
    }
    //ParseClient.Instance.RequestPasswordResetAsync(useremail);
    //ParseClient.Instance.LogInWithAsync(username, password);
    //ParseClient.Instance.SignUpWithAsync(username, password);



    public static T MapFromParseObjectToClassObject<T>(ParseObject parseObject) where T : new()
    {
        var model = new T();
        var properties = typeof(T).GetProperties();

        foreach (var property in properties)
        {
            try
            {

                //if (property.Name == "TaskId")
                //{

                //    var value = parseObject.ObjectId;
                //    if (value != null)
                //        Debug.WriteLine(value.GetType());
                //        Debug.WriteLine(value);
                //    property.SetValue(model, value);
                    
                //    continue; // Skip the rest of the loop for this property
                //}

                // Check if the ParseObject contains the property name
                if (parseObject.ContainsKey(property.Name))
                {
                    var value = parseObject[property.Name];

                    if (value != null)
                    {
                        // Handle special types like DateTimeOffset
                        if (property.PropertyType == typeof(DateTimeOffset) && value is DateTime dateTime)
                        {
                            property.SetValue(model, new DateTimeOffset(dateTime));
                            continue;
                        }

                        // Handle string as string
                        if (property.PropertyType == typeof(string) && value is string objectIdStr)
                        {
                            property.SetValue(model, new string(objectIdStr));
                            continue;
                        }

                        //For other types, directly set the value if the property has a setter
                        if (property.CanWrite && property.PropertyType.IsAssignableFrom(model.GetType()))

                        {
                            property.SetValue(model, value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log and skip the property
                Debug.WriteLine($"Error mapping property '{property.Name}': {ex.Message}");
            }
        }

        return model;
    }


    public static ParseObject MapToParseObject<T>(T model, string className)
    {
        var parseObject = new ParseObject(className);

        // Get the properties of the class
        var properties = typeof(T).GetProperties();

        foreach (var property in properties)
        {
            try
            {
                if (!property.CanRead)
                    continue;

                var value = property.GetValue(model);

                if (value is null)
                {
                    if (property.PropertyType == typeof(string))
                    {
                        parseObject[property.Name] = string.Empty;
                    }
                    if (property.PropertyType == typeof(bool))
                    {
                        parseObject[property.Name] = false;
                    }
                    if (property.PropertyType == typeof(object))
                    {
                        parseObject[property.Name] = null;
                    }
                    if (property.PropertyType == typeof(DateTime))
                    {
                        parseObject[property.Name] = null;
                    }
                    continue;
                }
                // Handle special types like DateTimeOffset
                if (property.PropertyType == typeof(DateTimeOffset))
                {
                    var val = (DateTimeOffset)value;
                    parseObject[property.Name] = val.Date;
                    continue;
                }

                // Handle string as string (required for Parse compatibility)
                if (property.PropertyType == typeof(string))
                {
                    parseObject[property.Name] = value.ToString();
                    continue;
                }

                // For other types, directly set the value
                parseObject[property.Name] = value;
            }
            catch (Exception ex)
            {
                // Log the exception for this particular property, but continue with the next one
                Debug.WriteLine($"Error when mapping property '{property.Name}': {ex.Message}");
            }
        }

        return parseObject;
    }


}