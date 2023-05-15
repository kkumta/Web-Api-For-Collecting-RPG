namespace WebApiForCollectingRPG.Services;

public interface IMasterService
{
    public void LoadItemListAsync();
    public void LoadItemAttributeListAsync();
    public void LoadAttendanceCompensationAsync();
    public void LoadInAppProductListAsync();
    public void LoadStageItemListAsync();
    public void LoadStageAttackNpcListAsync();
    public void LoadTotalStageCountAsync();
}