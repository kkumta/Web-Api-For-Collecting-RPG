namespace WebApiForCollectingRPG.Services;

public interface IMasterDb
{
    public void GetItemList();
    public void GetItemAttributeList();
    public void GetAttendanceCompensation();
    public void GetInAppProductListAsync();
}