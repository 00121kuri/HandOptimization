using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GraspingOptimization;
using Unity.VisualScripting;
using TMPro;

namespace GraspingOptimization
{
    public class ListViewController : MonoBehaviour
    {
        public RectTransform content;
        public GameObject listItemPrefab;

        public List<string> listItems = new List<string>();

        private float itemHeight;

        [SerializeField]
        private TMP_InputField inputField;

        // Start is called before the first frame update
        void Start()
        {
            // 高さを取得
            GameObject item = GameObject.Instantiate(listItemPrefab);
            RectTransform itemRect = item.GetComponent<RectTransform>();
            itemHeight = itemRect.sizeDelta.y;
            Destroy(item);
            ClearListView();
            UpdateListView();
        }

        private void UpdateListView()
        {
            ClearListView();
            // リストのアイテムを更新
            for (int i = 0; i < listItems.Count; i++)
            {
                GameObject item = GameObject.Instantiate(listItemPrefab);
                item.transform.SetParent(content.transform, false);
                item.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -itemHeight * i);
                Text itemText = item.GetComponentInChildren<Text>();
                itemText.text = listItems[i];
            }
        }

        public void ClearListView()
        {
            foreach (Transform child in content.transform)
            {
                Destroy(child.gameObject);
            }
        }

        public void AddListItem(string item)
        {
            listItems.Add(item);
            UpdateListView();
        }

        public void ClearListItem()
        {
            listItems.Clear();
            UpdateListView();
        }

        public void AddFromInputField()
        {
            if (inputField == null) return;
            AddListItem(inputField.text);
        }

        public List<string> GetList()
        {
            return listItems;
        }
    }
}