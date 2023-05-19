using System;

namespace WebApiForCollectingRPG.DAO;

public class AuthUser
{
    public String Email { get; set; } = "";
    public String AuthToken { get; set; } = "";
    public Int64 AccountId { get; set; } = 0;
    public Int64 PlayerId { get; set; } = 0;
    public String State { get; set; } = "";
}

public enum UserState
{
    Default = 0,
    Login = 1,
    Playing = 2
}