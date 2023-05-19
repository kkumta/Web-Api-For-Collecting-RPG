namespace WebApiForCollectingRPG.Services;

public interface IMasterService
{
    public void LoadItemList();
    public void LoadItemAttributeList();
    public void LoadAttendanceCompensation();
    public void LoadInAppProductList();
    public void LoadStageItemList();
    public void LoadStageAttackNpcList();
    public void LoadTotalStageCount();
}