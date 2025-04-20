using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Landscape2.Maebashi.Runtime.Dashboard
{
    public class HumanAvatarPoolingSystem : MonoBehaviour
    {
        // 配置候補となるゲームオブジェクト群
        [SerializeField]
        private GameObject[] humanAvatarPrefabs;

        /// <summary>
        /// アバターの数を取得する
        /// </summary>
        /// <returns></returns>
        public int CountAvatarKind()
        {
            return humanAvatarPrefabs.Length;
        }

        /// <summary>
        /// 指定されたIDに基づいてアバターを取得する
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public GameObject GetHumanAvatar(int id)
        {
            // IDに基づいてアバターを取得
            if (id < 0 || id >= humanAvatarPrefabs.Length)
            {
                Debug.LogError("Invalid ID: " + id);
                return null;
            }

            // アバターをインスタンス化して返す
            return Instantiate(humanAvatarPrefabs[id]);
        }

        /// <summary>
        /// アバターをプールに戻す
        /// </summary>
        /// <param name="avatar"></param>
        public void ReturnHumanAvatar(GameObject avatar)
        {
            // アバターを破棄する
            Destroy(avatar);
        }
        

    }

}
