using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Threading.Tasks;

namespace Landscape2.Maebashi.Runtime.Dashboard
{

    public class HumanFlowDataLoader : MonoBehaviour
    {
        [SerializeField]
        public TextAsset[] csvFiles;

        [SerializeField]
        private HumanFlowData[] humanFlowDatas;
        public HumanFlowData[] HumanFlowDatas => humanFlowDatas;

        [ContextMenu("LoadCSV")]
        private void LoadCSV()
        {
            Debug.Log("begin loading csv");
            Load();
            Debug.Log("end loading csv");
        }

        /// <summary>
        /// .csvは不要
        /// </summary>
        /// <param name="path"></param>
        public void Load()
        {
            var tasks = new List<Task>(csvFiles.Length);
            humanFlowDatas = new HumanFlowData[csvFiles.Length];
            for (int i = 0; i < csvFiles.Length; i++)
            {
                humanFlowDatas[i] = new HumanFlowData();
                humanFlowDatas[i].Load(csvFiles[i]);
            }
        }

    }
}
