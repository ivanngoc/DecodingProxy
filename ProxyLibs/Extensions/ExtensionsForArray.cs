using System.Collections.Generic;


namespace System
{

    public static class ExtensionsForArray
    {
        public static void ClearReferences<T>(this T[] arr) where T : class
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = default;
            }
        }

        public static void ForEach<T>(this T[] array, Action<T> action)
        {
            for (int i = 0; i < array.Length; i++)
            {
                action(array[i]);
            }
        }

        public static bool IsFirstMatch<T>(this T[] compare, T with) where T : IComparable<T>, IComparable, IEquatable<T>
        {
            for (var i = 0; i < compare.Length; i++)
            {
                if (with.Equals(compare[i])) return true;
            }
            return false;
        }
        public static void RemoveAtIndex<T>(this T[] self, out T[] link, int index)
        {
            link = new T[self.Length - 1];

            Array.Copy(self, link, index);
            Array.Copy(self, index + 1, link, index, self.Length - index - 1);
        }
        public static ref T ReplaceAtIndexWithLast<T>(this T[] self, int index)
        {
            self[index] = self[self.Length - 1];

            return ref self[index];
        }
        public static void Inverse<T>(this T[] array)
        {
            var count = array.Length / 2;

            for (var i = 0; i < count; i++)
            {
                var temp = array[i];
                var inverseIndex = array.Length - 1 - i;
                array[i] = array[inverseIndex];
                array[inverseIndex] = temp;
            }
        }

        public static void Clear<T>(this T[] array, int index)
        {
            for (int i = 0; i < index; i++)
            {
                array[i] = default;
            }
        }
        public static void Clear<T>(this T[] array)
        {
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = default;
            }
        }

        public static T IntersectFirst<T>(this T[] left, T[] right) where T : IEquatable<T>
        {
            for (var i = 0; i < left.Length; i++)
            {
                for (var j = 0; j < right.Length; j++)
                {
                    if (left[i].Equals(right[j]))
                    {
                        return left[i];
                    }
                }
            }
            return default;
        }

        public static T IntersectFirst<T>(this T[] left, T[] right, Func<T, T, bool> func)
        {
            for (var i = 0; i < left.Length; i++)
            {
                for (var j = 0; j < right.Length; j++)
                {
                    if (func(left[i], right[j]))
                    {
                        return left[i];
                    }
                }
            }
            return default;
        }

        public static int IndexOf<T>(this T[] self, T value) where T : IEquatable<T>
        {
            for (int i = 0; i < self.Length; i++)
            {
                if (value.Equals(self[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public static T[] PushAppend<T>(this T[] self, T value)
        {
            for (int i = 1; i < self.Length; i++)
            {
                self[i - 1] = self[i];
            }
            self[self.Length - 1] = value;

            return self;
        }

        public static T[] PushPrepand<T>(this T[] self, T valueToPrepand)
        {
            T temp = default;

            int count = self.Length;

            for (int i = 0; i < count; i++)
            {
                temp = self[i];

                self[i] = valueToPrepand;

                valueToPrepand = temp;
            }

            return self;
        }

        public static string ToStringLine<T>(this T[] self, string delimeter)
        {
            string result = default;

            for (int i = 0; i < self.Length; i++)
            {
                result += $"{self[i].ToString()}{delimeter}";
            }
            return result;
        }

        public static void SwapWithIndex<T>(this T[] self, int left, int right)
        {
            var temp = self[left];
            self[right] = self[left];
            self[left] = temp;
        }

        public static T[] Fill<T>(this T[] arr, T vaue)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = vaue;
            }
            return arr;
        }
        /// <summary>
        /// Уплотнение по маске. Зашивает "Дырки"-условно свободные элементы массива. ТАким образом все занятые ячейки перемещаются влева, а свободные вправо
        /// <see cref="ExtensionsIList.SwapRightToLeftAndSetDefaultRight{T}(IList{T}, int, int)"/>
        /// </summary>
        /// <returns>
        /// индекс последнего элемента
        /// </returns>
        public static int CompactByMask<T>(this T[] arr, bool[] mask, Action<T, int> updateIndex)
        {
            // перекладывание последнего занятого элемента с текущим получается пока эффективней
            throw new NotImplementedException();
        }

        public static bool IsEqualContent<T>(this T[] self, T[] to) where T : IEquatable<T>
        {
            if (self.Length != to.Length) return false;

            for (int i = 0; i < self.Length; i++)
            {
                if (!self[i].Equals(to[i])) return false;
            }
            return true;
        }
    }
}