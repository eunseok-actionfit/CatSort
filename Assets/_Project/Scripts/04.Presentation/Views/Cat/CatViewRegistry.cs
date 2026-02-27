using System.Collections.Generic;
using AES.Tools;
using Domain;          // CatId
using UnityEngine;

namespace Presentation.Mvvm
{
    public static class CatViewRegistry
    {
        private static readonly Dictionary<CatViewModel, CatView> _byVm = new();
        private static readonly Dictionary<CatId, CatView> _byId = new();

        public static bool TryGet(CatViewModel vm, out CatView view)
        {
            return _byVm.TryGetValue(vm, out view) && view != null;
        }

        public static bool TryGet(CatId id, out CatView view)
        {
            return _byId.TryGetValue(id, out view) && view != null;
        }

        public static CatView Get(CatId id)
        {
            return _byId.TryGetValue(id, out var view) ? view : null;
        }

        public static void Register(CatViewModel vm, CatView view)
        {
            if (vm == null || view == null) return;

            _byVm[vm] = view;
            _byId[vm.CatId] = view;
        }

        public static void Unregister(CatViewModel vm, CatView view)
        {
            if (vm == null) return;

            if (_byVm.TryGetValue(vm, out var current) && current == view)
                _byVm.Remove(vm);

            if (_byId.TryGetValue(vm.CatId, out var current2) && current2 == view)
                _byId.Remove(vm.CatId);
        }

        public static CatView GetOrCreate(
            CatViewModel vm,
            MonoContext catPrefabContext,
            Transform parent)
        {
            if (vm == null || catPrefabContext == null)
                return null;

            if (TryGet(vm, out var existing) && existing != null)
                return existing;

            var ctx = Object.Instantiate(catPrefabContext, parent);
            ctx.SetViewModel(vm);

            var view = ctx.GetComponent<CatView>();
            Register(vm, view);

            return view;
        }
    }
}