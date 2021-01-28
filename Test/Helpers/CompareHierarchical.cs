// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Test.Helpers
{
    public enum CompareStatuses { DifferentValue, DifferentCount, CouldNotCompare, SkippedBecauseIndexer, ExceptionOnGet, OneWasNull}
    public class CompareLog
    {
        public CompareLog(CompareStatuses status, string higherLevelName, string propName, Type propType)
        {
            Status = status;
            Name = higherLevelName;
            if (propName != null)
                Name = "." + propName;
            PropType = propType;
        }

        public CompareStatuses Status { get; }
        public string Name { get; }
        public Type PropType { get; }

        public override string ToString()
        {
            var result = $"{Status}: {Name}";
            if (Status == CompareStatuses.SkippedBecauseIndexer)
                result += $", type = {PropType.Name}";
            return result;
        }
    }
    
    public  class CompareHierarchical
    {
        private readonly HashSet<object> _foundBefore = new HashSet<object>();

        public List<CompareLog> LoggedDiffs { get; } = new List<CompareLog>();
        
        public void CompareTwoSimilarClasses(object class1, object class2, string higherLevelName = "")
        {
            if (class1 == null || _foundBefore.Contains(class1))
                return;
            if (class1.GetType().Namespace == "System")
                return;
            
            _foundBefore.Add(class1);

            var class1Type = class1.GetType();
            if (class1Type != class2.GetType())
                throw new Exception($"Class1 was {class1Type.Name} but class2 was {class2.GetType().Name}");

            var properties = class1Type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            foreach (var propertyInfo in properties)
            {
                var indexers = propertyInfo.GetIndexParameters();
                if (indexers.Length != 0)
                {
                    LoggedDiffs.Add(new CompareLog(CompareStatuses.SkippedBecauseIndexer, higherLevelName,
                        propertyInfo.Name, propertyInfo.PropertyType));
                    continue;
                }

                object c1Prop = null;
                object c2Prop = null;
                try
                {
                    c1Prop = propertyInfo.GetValue(class1);
                    c2Prop = propertyInfo.GetValue(class2);
                }
                catch (Exception e)
                {
                    LoggedDiffs.Add(new CompareLog(CompareStatuses.ExceptionOnGet, higherLevelName,
                        propertyInfo.Name, propertyInfo.PropertyType));
                    continue;
                }
                
                CompareProperty(c1Prop, c2Prop, $"{higherLevelName}.{propertyInfo.Name}");
            }
        }
        
        //------------------------------------------------
        //private methods

        private void CompareProperty(object prop1, object prop2, string higherLevelName)
        {
            if (prop1 == null || prop2 == null)
            {
                if (prop1 != prop2)
                    LoggedDiffs.Add(new CompareLog(CompareStatuses.OneWasNull, higherLevelName,
                        null, prop1.GetType()));

                return;
            }
            
            var prop1Type = prop1.GetType();
            if (prop1Type != prop2.GetType())
                throw new Exception($"Property1 was {prop1Type.Name} but Property1 was {prop2.GetType().Name}");

            if (prop1 is IEnumerable<object> c1PropIEnumerable)
            {
                var c2PropIEnumerable = (prop2 as IEnumerable<object>);
                CompareTwoIEnumerables(c1PropIEnumerable, c2PropIEnumerable, $"{higherLevelName}");
            }
            if (prop1.GetType().IsClass)
            {
                CompareTwoSimilarClasses(prop1, prop2, higherLevelName);
            }
            if (prop1 is IComparable prop1Comparable)
            {
                var compareResult = prop1Comparable.CompareTo((IComparable)prop2);
                if (compareResult != 0)
                    LoggedDiffs.Add(new CompareLog(CompareStatuses.DifferentValue, higherLevelName,
                        null, prop1.GetType()));
            }
            else
            {
                LoggedDiffs.Add(new CompareLog(CompareStatuses.CouldNotCompare, higherLevelName,
                    null, prop1.GetType()));
            }
        }
        
        private void CompareTwoIEnumerables(IEnumerable<object> c1PropIEnumerable, IEnumerable<object> c2PropIEnumerable,
            string higherLevelName)
        {

            if (c1PropIEnumerable == null || c2PropIEnumerable == null)
                throw new Exception($"c1PropIEnumerable was {c1PropIEnumerable.GetType().Name} but c2PropIEnumerable was {c2PropIEnumerable.GetType().Name}");

            var c1List = EnumerableToList(c1PropIEnumerable);
            var c2List = EnumerableToList(c2PropIEnumerable);

            if (c1List.Count != c2List.Count)
            {
                LoggedDiffs.Add(new CompareLog(CompareStatuses.DifferentCount, higherLevelName,
                    null, c1PropIEnumerable.GetType()));
                return;
            }
            

            for (int i = 0; i < c1List.Count; i++)
            {
                CompareProperty(c1List[i], c2List[i], $"{c1PropIEnumerable.GetType().Name} {higherLevelName}[{i}]");
            }
        }

        private List<object> EnumerableToList( IEnumerable<object> propEnumerable)
        {
            var list = new List<object>();

            foreach (var item in propEnumerable)
            {
                list.Add(item);
            }

            return list;
        }
    }
}