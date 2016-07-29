using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSNC.RepoAV.RepDBAccess
{
	public enum TaskType : short
	{
		Unknown = -1,
		Download = 0,
		Recode = 1,
		Remove = 2,
		RemoveFile = 3,
		AddMaterial = 4,
		RemoveMaterial = 5,
		UpdateMaterial = 6,
		SyncFormatMetadata = 7,
        FixFormatErrors = 8,
        FixReplication = 9,
        RemoveOldMaterials = 10,
		ValidateRepository = 11,
        AddFormat = 12,
        UpdateFormat = 13,
        RemoveFormat = 14,
        RemoveOldTasks = 15
	}
}
