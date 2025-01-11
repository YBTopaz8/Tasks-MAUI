

using CommunityToolkit.Mvvm.Input;
using System.Reactive.Linq;

namespace TasksCM;
public partial class ViewModel : ObservableObject
{
    [ObservableProperty]
    public partial TaskObject? SingleTask { get; set; }

    [ObservableProperty]
    public partial bool IsLogged { get; set; } = false;

    ParseUser? CurrentUser { get; set; }
    public ParseLiveQueryClient? LiveClient { get; set; }
    [ObservableProperty]
    public partial bool IsConnected { get; set; } = false;
    [ObservableProperty]
    public partial ObservableCollection<TaskObject> AllTasks { get; set; } = Enumerable.Empty<TaskObject>().ToObservableCollection();
    public ViewModel()
    {
        ParseLiveQueryClient client = new();

        CurrentUser = new ParseUser();
    }

    
    public async Task LoginUser(string username="as", string password="yvan")
    {
        username = "as";
        password = "Yvan"; 
        var user = await ParseStaticMethods.LoginUser(username, password);
        
        if (user != null)
        {
            await Shell.Current.DisplayAlert("Success", $"Welcome back {username}", "OK");
            CurrentUser = user;            
            await LoadTasks();
            IsConnected = true;
        }
        else
        {
            await Shell.Current.DisplayAlert("Error", "Invalid username or password", "OK");
        }
    }

    public async Task SignUpUser(string username, string password, string email)
    {
        await ParseStaticMethods.SignUpUser(username, password, email);
        await Shell.Current.DisplayAlert("Success", "Please check your email to confirm ", "OK");
        ParseClient.Instance.LogOut();
    }

    void SetupLiveQuery()
    {
        try
        {
            // Initialize Live Query client
            LiveClient = new ParseLiveQueryClient();

            // Queries for TaskUpdate and TaskObject
            var taskUpdateQuery = ParseClient.Instance.GetQuery("TaskObject");
            
            LiveClient.RemoveAllSubscriptions();

            // Subscriptions
            var taskUpdateSubscription = LiveClient.Subscribe(taskUpdateQuery);            

            // Handle connection retries
            SetupConnectionHandling();

            LiveClient.OnObjectEvent
                .Where(e => e.subscription == taskUpdateSubscription)
                .GroupBy(e=>e.evt)
                
                .SelectMany(group =>
                 {
                     if (group.Key == Subscription.Event.Create)
                     {
                         return group;
                     }
                     else
                     {
                         //do something with group !
                         // Pass through other events without throttling
                         return group;
                     }
                 })
            .Subscribe(e =>
            {
                ProcessEvent(e, 1);
            });

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SetupLiveQuery Error: {ex.Message}");
        }
    }
    void SetupLiveQuery(string taskId=null)
    {
        try
        {
            // Initialize Live Query client
            LiveClient = new ParseLiveQueryClient();
            LiveClient.RemoveAllSubscriptions();
            // Queries for TaskUpdate and TaskObject
            var taskUpdateQuery = ParseClient.Instance.GetQuery("TaskUpdate");                

            // Subscriptions
            var taskUpdateSubscription = LiveClient.Subscribe(taskUpdateQuery);
            
            // Handle connection retries
            SetupConnectionHandling();

            LiveClient.OnObjectEvent
                .Where(e => e.subscription == taskUpdateSubscription)
                .Subscribe(e =>
                {
                    //whenever anything happens to the class TaskUpdate, the ProcessEvent will be called automatically
                    ProcessEvent(e, 0);
                });

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SetupLiveQuery Error: {ex.Message}");
        }
    }

    void SetupConnectionHandling()
    {
        // Ensure reconnection and handle connection retries
        //

        int retryDelaySeconds = 5;
        int maxRetries = 10;

        LiveClient.OnConnected
            .Do(_ => Debug.WriteLine("LiveQuery connected."))
            .RetryWhen(errors =>
                errors.Zip(Observable.Range(1, maxRetries), (error, attempt) => (error, attempt))
                    .SelectMany<(Exception error, int attempt), long>(tuple =>
                    {
                        if (tuple.attempt > maxRetries)
                        {
                            Debug.WriteLine($"Max retries reached. Error: {tuple.error.Message}");
                            return Observable.Throw<long>(tuple.error);
                        }

                        Debug.WriteLine($"Retry attempt {tuple.attempt} after {retryDelaySeconds} seconds...");
                         // Attempt reconnect
                        return Observable.Timer(TimeSpan.FromSeconds(retryDelaySeconds));
                    })
            )
            .Subscribe(
                _ => Debug.WriteLine("Reconnected successfully."),
                ex => Debug.WriteLine($"Failed to reconnect: {ex.Message}")
            );

        LiveClient.OnError
            .Do(ex =>
            {
                Debug.WriteLine($"LiveQuery Error: {ex.Message}");
                 // Attempt reconnect on error
            })
            .Subscribe();

        LiveClient.OnDisconnected
            .Do(info =>
            {
                if (info.userInitiated)
                {
                    Debug.WriteLine("User disconnected.");
                }
                else
                {
                    Debug.WriteLine("Server disconnected.");
                    // // Attempt reconnect
                }
                IsConnected = false;
            })
            .Subscribe();
    }


    public CollectionView TaskUpdatesColView;

    [ObservableProperty]
    public partial ObservableCollection<TaskUpdate> TaskUpdatess { get; set; } = new();

    void ProcessEvent((Subscription.Event evt, object objectDictionnary, Subscription subscription) e, int subType) //0 for TaskUpdate, 1 for TaskObject
    {
        Debug.WriteLine($"We have {LiveClient.Subscriptions.Count} Subs");
        var objData = e.objectDictionnary as Dictionary<string, object>;
        TaskUpdate taskupdate;
        TaskObject taskObject;

        switch (e.evt)
        {
            case Subscription.Event.Enter:
                Debug.WriteLine("Entered");
                break;

            case Subscription.Event.Leave:
                Debug.WriteLine("Left");
                break;

            case Subscription.Event.Create:

                switch (subType)
                {
                    case 0: //0 for TaskUpdate
                        taskupdate = ParseLiveQueryClient.ObjectMapper.MapFromDictionary<TaskUpdate>(objData);
                        if (taskupdate.UpdateType == "4" ||taskupdate.UpdateType == "5" )
                        {
                            if (taskupdate.UpdateSender != CurrentUser.ObjectId)
                            {
                                taskupdate.UpdateType = "5";
                            }
                            TaskUpdatess.Add(taskupdate);

                            TaskUpdatesColView.ScrollTo(TaskUpdatess.Count - 1);
                        }
                        break;
                    case 1:
                        taskObject= ParseLiveQueryClient.ObjectMapper.MapFromDictionary<TaskObject>(objData);                        
                        AllTasks.Add(taskObject);                        
                        break;
                    default:
                        break;
                }
                break;

            case Subscription.Event.Update:
                switch (subType)
                {
                    case 0:

                        break;
                    case 1:
                        taskObject = ParseLiveQueryClient.ObjectMapper.MapFromDictionary<TaskObject>(objData);
                        var objToDeletes = AllTasks.FirstOrDefault(x => x.TaskId == taskObject.TaskId);

                        if (objToDeletes != null)
                        {
                            AllTasks.Remove(objToDeletes);
                        }
                        else
                        {
                            AllTasks.Add(taskObject);
                        }
                        break;
                    default:
                        break;
                }
               
                break;

            case Subscription.Event.Delete:
                taskupdate = ParseLiveQueryClient.ObjectMapper.MapFromDictionary<TaskUpdate>(objData);
                var objToDelete = TaskUpdatess.FirstOrDefault(x => x.TaskUpID == taskupdate.TaskUpID);

                if (objToDelete != null)
                {
                    TaskUpdatess.Remove(objToDelete);
                }
                if (TaskUpdatess.Count > 1)
                {
                    
                }
                break;

            default:
                Debug.WriteLine("Unhandled event type.");
                break;
        }

        Debug.WriteLine($"Processed {e.evt} on object {objData?.GetType()}");
    }

    [ObservableProperty]
    public partial TaskObject CurrentTask { get; set; }
    
    [ObservableProperty]
    public partial ParseObject CurrentParseTask { get; set; }

    [RelayCommand]
    public async Task SelectTaskOnUI(TaskObject task)
    {
        if (task == CurrentTask || string.IsNullOrEmpty( task.UserId ))
        {
            return;
        }
        else
        {
            TaskUpdatess = new();

            var queryy = ParseClient.Instance.GetQuery("TaskUpdate")
                .WhereEqualTo("TaskID", task.TaskId);
            

            var actualTasks = await queryy.FindAsync();


            if (actualTasks is null || actualTasks.Count() == 0)
            {
                return;
            }

            foreach (var item in actualTasks)
            {
                // Map each ParseObject (TaskUpdate) to your TaskUpdate class
                var obj = ParseStaticMethods.MapFromParseObjectToClassObject<TaskUpdate>(item);
                if (obj.UpdateType == "4" || obj.UpdateType == "5" )
                {
                    TaskUpdatess.Add(obj);
                }
            }
            SelectedTaskObject.TaskId = task.TaskId;
            // Setup live query or any other logic
            SetupLiveQuery(task.TaskId);
        }

    }

    private async Task LoadTasks()
    {
        AllTasks = new(); 
        var query = ParseClient.Instance.GetQuery("TaskObject")
            .WhereEqualTo("UserId", CurrentUser.ObjectId);

        IEnumerable<ParseObject>? AllItems = await query.FindAsync();
        var actualTasks = AllItems.ToList();

        if (actualTasks is null || actualTasks.Count == 0)
        {
            return;
        }
        foreach (var item in actualTasks) 
        {
            TaskObject? obj = ParseStaticMethods.MapFromParseObjectToClassObject<TaskObject>(item);
            AllTasks.Add(obj);
        }
        SetupLiveQuery();
    }


    [ObservableProperty]
    public partial TaskObject SelectedTaskObject { get; set; } = new();
    public async Task UpdateTask(string key)
    {
        try
        {
            // Step 1: Query the existing object by a unique identifier (e.g., Username, Msg)
            var query = ParseClient.Instance.GetQuery("TaskObject")
                .WhereEqualTo("TaskUpID", key ); // Filter condition
            
            var existingChat = await query.FirstAsync(); // Fetch the first matching object
                        
            existingChat["Msg"] = "NowYB"; // Update message or other properties as needed
            
            // Step 3: Save the updated object back to the server
            await existingChat.SaveAsync();

            Debug.WriteLine("Task updated successfully!");
        }
        catch (ParseFailureException ex) when (ex.Code == ParseFailureException.ErrorCode.ObjectNotFound)
        {
            Debug.WriteLine("No matching object found to update.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating message: {ex.Message}");
        }
    }
    [RelayCommand]
    public async void DeleteTask()
    {        
        // Step 1: Query the existing object by a unique identifier (e.g., Username, Msg)
        var query = ParseClient.Instance.GetQuery("TaskObject")
            .WhereEqualTo("TaskId", SelectedTaskObject.TaskId); // Filter condition 
        var parseTUpdate = await query.FirstOrDefaultAsync();
        if (parseTUpdate is not null)
        {
            _ = parseTUpdate.DeleteAsync();
        }

        AllTasks.Remove(SelectedTaskObject);
        //Heads up. handling cross device deletion is tricky. Make sure BOTH devices produce the same UniqueKey (or similar) and that both will that data when they create/update.
    }

    public async Task UpdateTaskState()
    {
        await UpSertTask(SelectedTaskObject);
    }
    public async Task UpSertTask(TaskObject specificTaskObs)
    {
        try
        {
            var newIdIfNeeded = Guid.NewGuid().ToString();
            // Step 2: Create TaskObject
            var specificTask = new ParseObject("TaskObject");
            specificTask["IsDone"] = specificTaskObs.IsDone;
            specificTask["UserId"] = CurrentUser.ObjectId;
            specificTask["Name"] = specificTaskObs.Name;
            specificTask["Deadline"] = specificTaskObs.Deadline;
            specificTask["Description"] = specificTaskObs.Description;
            if (string.IsNullOrEmpty(specificTaskObs.TaskId))
            {
                specificTask["TaskId"] = newIdIfNeeded;
                specificTaskObs.TaskId=newIdIfNeeded;
            }
            else
            {
                specificTask["TaskId"] =  specificTaskObs.TaskId;
            }
            specificTask["AdditionalNotes"] = specificTaskObs.AdditionalNotes;

            await specificTask.SaveAsync(); // Save to ensure ObjectId is created

            var parseTask = await ParseClient.Instance.GetQuery("TaskObject")
                .WhereEqualTo("TaskId", specificTaskObs.TaskId)
                .FirstOrDefaultAsync();

            var taskUpID = Guid.NewGuid().ToString();
            // Step 1: Create TaskUpdate object
            var taskUpdate = new ParseObject("TaskUpdate");
            taskUpdate["UpdateType"] = string.IsNullOrEmpty(specificTaskObs.TaskId)? "0" : "1";
            taskUpdate["UpdateSender"] = CurrentUser.ObjectId;
            taskUpdate["UpdateSenderName"] = CurrentUser.Username;
            taskUpdate["UpdateContent"] = "Task created";
            taskUpdate["UpdateNote"] = specificTaskObs.AdditionalNotes;
            taskUpdate["UpdateDate"] = DateTime.Now;
            taskUpdate["TaskUpID"] = taskUpID;
            taskUpdate["TaskPlatForm"] = $"Platform is: {DeviceInfo.Platform} + {DeviceInfo.Idiom}";
            taskUpdate["TaskID"] = specificTaskObs.TaskId;
        
            await taskUpdate.SaveAsync();
            
            Debug.WriteLine($"Task and TaskUpdate created successfully with Task ID: {specificTask.ObjectId}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error creating task with update: {ex.Message}");
        }
    }

    [ObservableProperty]
    public partial string? Note { get; set; }=string.Empty;
    [RelayCommand]
    public async Task SendNote()
    {
        // Step 1: Create TaskUpdate object
        var taskUpdate = new ParseObject("TaskUpdate");
        taskUpdate["UpdateType"] = "4"; // Task creation update
        taskUpdate["UpdateSender"] = CurrentUser.ObjectId;
        taskUpdate["UpdateSenderName"] = CurrentUser.Username;
        taskUpdate["UpdateContent"] = Note;        
        taskUpdate["UpdateDate"] = DateTime.Now;
        taskUpdate["TaskPlatForm"] = $"Platform is: {DeviceInfo.Platform} + {DeviceInfo.Idiom}";
        taskUpdate["TaskID"] = SelectedTaskObject.TaskId;
        
        

        await taskUpdate.SaveAsync();


        Note=string.Empty;
    }



}
