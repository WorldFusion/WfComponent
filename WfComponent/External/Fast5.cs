using HDF5DotNet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WfComponent.External
{
    public class Fast5
    {
        public static string flowcell = "flow";
        public static string sequence = "sequenc";

        public IDictionary<string, string> attributes;
        public List<string> logMesage;
        public string errorMessage = string.Empty;

        public Fast5(string h5filePath)
        {
            // grobals
            this.attributes = new Dictionary<string, string>();
            this.logMesage = new List<string>();
            try
            {
                H5FileId fileID = H5F.open(h5filePath, H5F.OpenMode.ACC_DEBUG);
                H5GroupId h5root = H5G.open(fileID, "/");
                long nunObj = H5G.getNumObjects(h5root);   // long: root-id


                for (long i = 0; i < nunObj; i++)
                {
                    string pore = H5G.getObjectNameByIndex(h5root, (ulong)i); // 最初はPore-id-string
                    ReadObjctName(H5G.open(h5root, pore));
                    if (attributes.Where(s => s.Key.StartsWith(Fast5.sequence)).Any())
                        break;
                }

            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }
        }


        private void ReadObjctName(H5GroupId groupId )
        {
            // Group が持っている Attribute を 精査 
            var objCnt  = H5G.getNumObjects(groupId);
            for (long i = 0; i < objCnt; i++)
            {
                var objName = H5G.getObjectNameByIndex(groupId, (ulong)i);
                // System.Diagnostics.Debug.WriteLine(objName);  // Row/chanel_is・・・
                logMesage.Add("FAST5 object name : " + objName);
                GetAttributes(H5G.open(groupId, objName), objName);
            }
        }

        public void GetAttributes(H5GroupId id , string attributeName)
        {
            // var objCnt = H5G.getNumObjects(id);
            // if (objCnt > 0) ReadObjctName(id); // 再起的 Group read

            int attributeNum = H5A.getNumberOfAttributes(id);
            if (attributeNum <= 0) return;
            for (int i = 0; i < attributeNum; i++)
            {
                H5AttributeId attrubuteId = H5A.openIndex(id, i);
                H5AttributeInfo info = H5A.getInfo(attrubuteId);
                var name = H5A.getName(attrubuteId);


                var s = new byte[1000]; // 取り敢えずのバッファ
                H5DataTypeId attributeType = H5A.getType(attrubuteId);
                H5A.read<byte>(attrubuteId, attributeType, new H5Array<byte>(s));
                var attribute = System.Text.Encoding.UTF8.GetString(s).Trim('\0');

                // System.Diagnostics.Debug.WriteLine(name + " : " + attribute);
                logMesage.Add("FAST5 : " + name + " -> " + attribute);
                if (!attributes.ContainsKey(name))
                    this.attributes.Add(name, attribute);
            }
        }

    }
}
