namespace TasksCM.Models;
public partial class UserModel : ObservableObject
{
    [ObservableProperty]
    public partial string?  ObjectId { get; set; }
    [ObservableProperty]
    public partial string?  CreatedAt { get; set; }
    [ObservableProperty]
    public partial string?  UpdatedAt { get; set; }
    [ObservableProperty]
    public partial string?  Username { get; set; }
    [ObservableProperty]
    public partial string?  Password { get; set; }
    [ObservableProperty]
    public partial string?  Email { get; set; }
   
}

public partial class Device : ObservableObject
{
    [ObservableProperty]
    public partial string?  DeviceName { get;set;}
    
    [ObservableProperty]
    public partial string?  DeviceForm { get;set;}
    
    [ObservableProperty]
    public partial string?  UserId {get;set;}
    
}

public partial class TaskObject : ObservableObject
{
    
    [ObservableProperty]
    public partial string? TaskId { get; set; }

    [ObservableProperty]
    public partial string? Name { get; set; }
    [ObservableProperty]
    public partial string? Description {get;set;}
    [ObservableProperty]
    public partial string? AdditionalNotes {get;set;}
    [ObservableProperty]
    public partial DateTime Deadline {get;set;}
    [ObservableProperty]
    public partial bool IsDone {get;set;}
    [ObservableProperty]
    public partial TaskUpdate TaskUpdate { get; set; }
    [ObservableProperty]
    public partial string? UpdateCode {get;set;}
    [ObservableProperty]
    public partial string? UserId {get;set;}

}


public partial class TaskUpdate : ObservableObject
{
    [ObservableProperty]
    public partial string? TaskUpID { get; set; }
    [ObservableProperty]
    public partial string? UpdateType {get;set; }     //0 for task creation, 1 for task update, 2 for task deletion, 3 for task completion, 4 for user sent msg, 5 for user received msg
    [ObservableProperty]
    public partial string? UpdateSender {get;set;}   
    [ObservableProperty]
    public partial string? UpdateSenderName {get;set;}   
    [ObservableProperty]
    public partial string? UpdateContent {get;set;}    
    [ObservableProperty]
    public partial DateTime UpdateDate {get;set;}  
    [ObservableProperty]
    public partial string? UpdateNote { get; set; }
    [ObservableProperty]
    public partial string? UserId { get; set; }
    [ObservableProperty]
    public partial string? TaskId { get; set; }
    [ObservableProperty]
    public partial string? TaskPlatForm { get; set; } 

}