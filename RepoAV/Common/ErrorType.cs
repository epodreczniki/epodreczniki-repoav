using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace PSNC.RepoAV.Common
{
	public enum ErrorType : int
	{
		[EnumMember]
		Success = 1,

		[EnumMember]
		InvalidParameter = -1,

		[EnumMember]
		ExecutionTimeout = -2,				

		[EnumMember]
		SystemOverloaded = -5,

		[EnumMember]
		Runtime = -6,

		[EnumMember]
		General = -9,




		[EnumMember]
		NotFound = -41,

		[EnumMember]
		AlreadyExists = -43,

		[EnumMember]
		StateChangedMeanwhile = -44,

		[EnumMember]
		IncompatibleState = -45,


		[EnumMember]
		NotEnoughFreeSpace = -48,


		[EnumMember]
		MaterialTypeNotFound = -51,

		[EnumMember]
		MaterialNotFound = -52,

		[EnumMember]
		FormatNotFound = -53,

		[EnumMember]
		FormatWithProfileExists = -54,

		[EnumMember]
		FormatGroup4MaterialExists = -55,

		[EnumMember]
		SubtitleFormatNotFound = -56,

		[EnumMember]
		Format4GroupExists = -57,

		[EnumMember]
		FormatGroupNotFound = -58,

		[EnumMember]
		ProfileNotFound = -59,

		[EnumMember]
		FormatTypeNotFound = -60,

		[EnumMember]
		NodeNotFound = -61,

		[EnumMember]
		NodeDisabled = -62,

		[EnumMember]
		WrongConfiguration = -63,

        [EnumMember]
        FormatExists = - 64,

		[EnumMember]
		TransmittingFailed = -101,

		[EnumMember]
		OperationAborted = -102,

		[EnumMember]
		MaterialFormatDBError = -103,

		[EnumMember]
		RepoDBError = -104,

		[EnumMember]
		FileDeleteFailed = -105,

		[EnumMember]
		FileMoveFailed = -106

	}
}
