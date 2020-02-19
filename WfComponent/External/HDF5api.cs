using HDF5DotNet;
using System;

namespace WfComponent.External
{
    public static class HDF5api
    {

        public static (string sequenceKitId, string flowcellType, string errorMessage) 
            GetSequenceKitFlowcellType(string h5filePath)
        {
            var sequencekit = string.Empty;
            var flowcelltype = string.Empty;
            var errorMes = string.Empty;
            try
            {
                H5FileId fileID = H5F.open(h5filePath, H5F.OpenMode.ACC_RDONLY);
                H5DataTypeId typeID = H5T.create(H5T.CreateClass.STRING, -1);
                H5GroupId groupId = H5G.open(fileID, "/UniqueGlobalKey/context_tags");

                H5AttributeId sequencing_kit_attributeId = H5A.openName(groupId, "sequencing_kit");
                H5AttributeId flowcell_type_attributeId = H5A.openName(groupId, "flowcell_type");

                H5DataTypeId sequencing_kit_attributeType = H5A.getType(sequencing_kit_attributeId);
                H5DataTypeId flowcell_type_attributeType = H5A.getType(flowcell_type_attributeId);

                var s = new byte[1000]; // 取り敢えずのバッファ

                H5A.read<byte>(sequencing_kit_attributeId, sequencing_kit_attributeType, new H5Array<byte>(s));
                sequencekit = System.Text.Encoding.UTF8.GetString(s).Trim('\0');

                H5A.read<byte>(flowcell_type_attributeId, flowcell_type_attributeType, new H5Array<byte>(s));
                flowcelltype = System.Text.Encoding.UTF8.GetString(s).Trim('\0');

            }
            catch (Exception e)
            {
                errorMes = e.Message;
            }
            // return (string.Empty, string.Empty);
            return (sequencekit, flowcelltype, errorMes);
        }

        public static bool IsContainSequence(string h5filePath)
        {
            try
            {
                H5FileId fileID = H5F.open(h5filePath, H5F.OpenMode.ACC_RDONLY);
                H5DataTypeId typeID = H5T.create(H5T.CreateClass.STRING, -1);
                H5GroupId groupId = H5G.open(fileID, "/Analyses");
            }
            catch
            {   // 含まれていない場合はException となる。
                return false;
            }

            return true;
        }

    }
}
