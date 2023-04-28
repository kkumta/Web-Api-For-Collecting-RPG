using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApiForCollectingRPG.Dtos.MasterData;

namespace WebApiForCollectingRPG.Services;

public interface IMasterDb
{
    public void GetItemList();
    public void GetItemAttributeList();
}
