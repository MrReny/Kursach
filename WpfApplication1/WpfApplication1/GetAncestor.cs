//https://github.com/Egor92/TrumpSoftware-Foundation/blob/develop/TrumpSoftware.Shared/Helpers/UIHelper.cs#L63

using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace WpfApplication1

{
    public static class UIHelper
    {
        #region VisualTreeHelper methods

        public static DependencyObject GetParent(this DependencyObject source)
        {
            var parent = VisualTreeHelper.GetParent(source);
            if (parent == null)
            {
                var frameworkElement = source as FrameworkElement;
                if (frameworkElement != null)
                    parent = frameworkElement.Parent;

                var frameworkContentElement = source as FrameworkContentElement;
                if (frameworkContentElement != null)
                    parent = frameworkContentElement.Parent;
            }
            return parent;
        }

        public static IEnumerable<DependencyObject> GetChildren(this DependencyObject source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var childrenCount = VisualTreeHelper.GetChildrenCount(source);
            for (int i = 0; i < childrenCount; i++)
            {
                yield return VisualTreeHelper.GetChild(source, i);
            }
        }

        public static T GetChildOfType<T>(this DependencyObject depObj) 
            where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }
        #endregion

        #region GetAncestor methods

        public static T GetAncestor<T>(this DependencyObject source, bool includingItself = false)
            where T : DependencyObject
        {
            return GetAncestor<T>(source, null, includingItself);
        }

        public static T GetAncestor<T>(this DependencyObject source, Func<T, bool> targetElementCondition, bool includingItself = false)
            where T : DependencyObject
        {
            return GetAncestor(source, targetElementCondition, null, includingItself);
        }

        public static T GetAncestor<T>(this DependencyObject source,
                                       Func<T, bool> targetElementCondition,
                                       Func<DependencyObject, bool> abortCondition,
                                       bool includingItself = false)
            where T : DependencyObject
        {
            Func<DependencyObject, bool> condition = GetNotGenericCondition(targetElementCondition);
            return (T) source.GetAncestor(typeof (T), condition, abortCondition, includingItself);
        }

        public static DependencyObject GetAncestor(this DependencyObject source, Type ancestorType, bool includingItself = false)
        {
            return source.GetAncestor(ancestorType, null, null, includingItself);
        }

        public static DependencyObject GetAncestor(this DependencyObject source,
                                                   Type ancestorType,
                                                   Func<DependencyObject, bool> targetElementCondition,
                                                   Func<DependencyObject, bool> abortCondition,
                                                   bool includingItself = false)
        {
            if (!typeof (DependencyObject).IsAssignableFrom(ancestorType))
                throw new ArgumentException("AncestorType must be assignable to 'System.Windows.DependencyObject'", "ancestorType");

            if (includingItself)
            {
                if (ancestorType.IsInstanceOfType(source))
                {
                    if (targetElementCondition == null || targetElementCondition(source))
                    {
                        return source;
                    }
                }
            }

            abortCondition = abortCondition ?? (x => false);
            if (abortCondition(source))
                return null;

            var parent = source.GetParent();
            if (parent == null)
                return null;

            return parent.GetAncestor(ancestorType, targetElementCondition, abortCondition, true);
        }

        #endregion
        private static Func<DependencyObject, bool> GetNotGenericCondition<T>(Func<T, bool> targetElementCondition)
            where T : DependencyObject
        {
            return x =>
            {
                if (targetElementCondition == null)
                    return true;
                return targetElementCondition((T) x);
            };
        }
    }
}
