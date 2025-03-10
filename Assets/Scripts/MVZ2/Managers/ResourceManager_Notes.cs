﻿using System.Collections.Generic;
using System.Linq;
using MVZ2.Metas;
using PVZEngine;
using UnityEngine;

namespace MVZ2.Managers
{
    public partial class ResourceManager : MonoBehaviour
    {
        #region 元数据列表
        public NoteMetaList GetNoteMetaList(string nsp)
        {
            var modResource = main.ResourceManager.GetModResource(nsp);
            if (modResource == null)
                return null;
            return modResource.NoteMetaList;
        }
        #endregion

        #region 元数据
        public NoteMeta GetNoteMeta(NamespaceID note)
        {
            var modResource = main.ResourceManager.GetModResource(note.SpaceName);
            if (modResource == null)
                return null;
            return modResource.NoteMetaList.metas.FirstOrDefault(m => m.id == note.Path);
        }
        #endregion
        public NamespaceID[] GetAllNotes()
        {
            return noteCache.ToArray();
        }
        private List<NamespaceID> noteCache = new List<NamespaceID>();
    }
}
