﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MVZ2.UI
{
    public class ElementListUI : MonoBehaviour
    {
        public void updateList(
            int count,
            Action<int, RectTransform> onUpdate = null,
            Action<RectTransform> onCreateOrEnable = null,
            Action<RectTransform> onDestroyOrDisable = null,
            bool dontDestroy = false,
            bool rebuild = false)
        {
            if (_template.transform.parent == _listRoot)
            {
                _template.gameObject.SetActive(false);
            }

            int maxNum = Math.Max(_itemList.Count, count);

            for (int i = 0; i < maxNum; i++)
            {
                if (i < count) // 应当出现在列表中
                {
                    RectTransform item;
                    if (i >= _itemList.Count) // 目前没有这个项
                    {
                        //创建列表项
                        item = CreateItem();
                        Add(item);
                        onCreateOrEnable?.Invoke(item);
                    }
                    else // 目前有这个项
                    {
                        item = _itemList[i];
                        if (!item.gameObject.activeSelf)
                        {
                            //激活
                            item.gameObject.SetActive(true);
                            onCreateOrEnable?.Invoke(item);
                        }
                    }
                    //更新
                    onUpdate?.Invoke(i, item);
                }
                else // 不应出现在列表中
                {
                    RectTransform item;
                    if (!dontDestroy) // 可以销毁
                    {
                        if (count < _itemList.Count) // 目前有这个项
                        {
                            // 销毁列表项
                            item = _itemList[count];
                            DestroyItem(item);
                            onDestroyOrDisable?.Invoke(item);
                        }
                    }
                    else // 不可以销毁
                    {
                        if (i < _itemList.Count) // 目前有这个项
                        {
                            item = _itemList[i];
                            if (item.gameObject.activeSelf)
                            {
                                // 禁用
                                item.gameObject.SetActive(false);
                                onDestroyOrDisable?.Invoke(item);
                            }
                        }
                    }
                }
            }
            if (!rebuild)
                return;
            foreach (var layoutGroup in _listRoot.GetComponentsInChildren<LayoutGroup>())
            {
                if (layoutGroup.transform == _listRoot)
                    continue;
                LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.transform as RectTransform);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(_listRoot);
            foreach (var layoutGroup in _listRoot.GetComponentsInParent<LayoutGroup>())
            {
                if (layoutGroup.transform == _listRoot)
                    continue;
                LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.transform as RectTransform);
            }
        }
        public void Add(RectTransform item)
        {
            _itemList.Add(item);
        }
        public bool Remove(RectTransform item)
        {
            return _itemList.Remove(item);
        }
        public void RemoveAt(int index)
        {
            _itemList.RemoveAt(index);
        }
        public RectTransform CreateItem()
        {
            var item = Object.Instantiate(_template, _listRoot);
            //激活
            item.gameObject.SetActive(true);
            return item;
        }
        public bool DestroyItem(RectTransform item)
        {
            if (Remove(item))
            {
                item.SetParent(null);
                Object.Destroy(item.gameObject);
                return true;
            }
            return false;
        }
        public int indexOf(RectTransform trans)
        {
            return _itemList.IndexOf(trans);
        }
        public int indexOf(Component comp)
        {
            return indexOf(comp.transform as RectTransform);
        }
        public RectTransform getElement(int index)
        {
            if (index < 0 || index >= _itemList.Count)
                return null;
            return _itemList[index];
        }
        public T getElement<T>(int index) where T : Component
        {
            return getElement(index)?.GetComponent<T>();
        }
        public RectTransform getTemplate()
        {
            return _template;
        }
        public T getTemplate<T>() where T : Component
        {
            return getTemplate()?.GetComponent<T>();
        }
        public IEnumerable<RectTransform> getElements()
        {
            return _itemList;
        }
        public IEnumerable<T> getElements<T>() where T : Component
        {
            foreach (var item in _itemList)
            {
                yield return item.GetComponent<T>();
            }
        }
        public int count => _itemList.Count;
        [SerializeField]
        private RectTransform _listRoot;
        [SerializeField]
        private RectTransform _template;
        [SerializeField]
        private List<RectTransform> _itemList = new List<RectTransform>();
    }
}
