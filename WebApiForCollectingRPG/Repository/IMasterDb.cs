using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApiForCollectingRPG.DAO.Master;
using WebApiForCollectingRPG.DTO.Mail;

namespace WebApiForCollectingRPG.Repository;

public interface IMasterDb
{
    public void GetItemList();
    public void GetItemAttributeList();
    public void GetAttendanceCompensation();
    public void GetInAppProductListAsync();
    public AttendanceCompensation GetAttendanceCompensationByCompensationId(Int16 compensationId);
}