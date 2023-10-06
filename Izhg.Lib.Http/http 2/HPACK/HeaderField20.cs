using System;
using System.Text;
using IziHardGames.Libs.HttpCommon.Attributes;
using IziHardGames.Libs.HttpCommon.Contracts;

namespace IziHardGames.Libs.ForHttp20.DecodingHeaders
{
    /// <summary>
    /// The HeaderField class.
    /// </summary>
    [HttpHeader]
    public class HeaderField20 : IComparable<HeaderField20>, IHttpHader
    {
        private readonly byte[] name;
        private readonly byte[] value;

        /// <summary>
        /// Section 4.1. Calculating Table Size
        /// The additional 32 octets account for an estimated
        /// overhead associated with the structure.
        /// </summary>
        public static readonly int HEADER_ENTRY_OVERHEAD = 32;

        /// <summary>
        /// The Name.
        /// </summary>
        public byte[] Name { get { return this.name; } }

        /// <summary>
        /// The Value.
        /// </summary>
        /// <value></value>
        public byte[] Value { get { return this.value; } }

        public string NameAsString { get { return Encoding.UTF8.GetString(this.name); } }
        public string ValueAsString { get { return Encoding.UTF8.GetString(this.value); } }
        /// <summary>
        /// The Size.
        /// </summary>
        /// <value></value>
        public int Size { get { return this.name.Length + this.value.Length + HEADER_ENTRY_OVERHEAD; } }

        /// <summary>
        /// This constructor can only be used if name and value are ISO-8859-1 encoded.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public HeaderField20(string name, string value)
        {
            this.name = Encoding.UTF8.GetBytes(name);
            this.value = Encoding.UTF8.GetBytes(value);
        }

        /// <summary>
        /// default constructor.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public HeaderField20(byte[] name, byte[] value)
        {
            this.name = (byte[])RequireNonNull(name);
            this.value = (byte[])RequireNonNull(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns>int</returns>
        public static int SizeOf(byte[] name, byte[] value)
        {
            return name.Length + value.Length + HEADER_ENTRY_OVERHEAD;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="anotherHeaderField"></param>
        /// <returns>int</returns>
        public int CompareTo(HeaderField20 anotherHeaderField)
        {
            var ret = CompareTo(this.name, anotherHeaderField.name);
            if (ret == 0)
            {
                ret = CompareTo(this.value, anotherHeaderField.value);
            }
            return ret;
        }

        private static int CompareTo(byte[] s1, byte[] s2)
        {
            var len1 = s1.Length;
            var len2 = s2.Length;
            var lim = Math.Min(len1, len2);

            var k = 0;
            while (k < lim)
            {
                var b1 = s1[k];
                var b2 = s2[k];
                if (b1 != b2)
                {
                    return b1 - b2;
                }
                k++;
            }
            return len1 - len2;
        }

        /// <summary>
        /// Check, if the header fields are equal.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>bool</returns>
        public override bool Equals(Object obj)
        {
            if (obj is HeaderField20 other)
            {
                if (other == this)
                {
                    return true;
                }
                var nameEquals = Equals(this.name, other.name);
                var valueEquals = Equals(this.value, other.value);
                return nameEquals && valueEquals;
            }

            return false;
        }
        /// <summary>
        /// A string compare that doesn't leak timing information.
        /// </summary>
        /// <param name="s1">S1.</param>
        /// <param name="s2">S2.</param>
        public static bool Equals(byte[] s1, byte[] s2)
        {
            if (s1.Length != s2.Length)
            {
                return false;
            }
            var c = 0;
            for (var i = 0; i < s1.Length; i++)
            {
                c |= (s1[i] ^ s2[i]);
            }
            return c == 0;
        }

        /// <summary>
        /// Gets the hashcode of this instance
        /// </summary>
        /// <returns>int</returns>
        public override int GetHashCode()
        {
            return this.name.GetHashCode() ^ this.value.GetHashCode();
        }

        /// <summary>
        /// Gets a formatted output of this header-field.
        /// </summary>
        /// <returns>string</returns>
        public override String ToString()
        {
            return String.Format("{0}: {1}", Encoding.UTF8.GetString(this.name), Encoding.UTF8.GetString(this.value));
        }

        public static bool operator ==(HeaderField20 left, HeaderField20 right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return object.ReferenceEquals(right, null);
            }
            return left.Equals(right);
        }

        public static bool operator !=(HeaderField20 left, HeaderField20 right)
        {
            return !(left == right);
        }

        public static bool operator >(HeaderField20 left, HeaderField20 right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(HeaderField20 left, HeaderField20 right)
        {
            return left.CompareTo(right) >= 0;
        }

        public static bool operator <(HeaderField20 left, HeaderField20 right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(HeaderField20 left, HeaderField20 right)
        {
            return left.CompareTo(right) <= 0;
        }
        /// <summary>
		/// Checks that the specified object reference is not {@code null}.
		/// </summary>
		/// <returns>The non null.</returns>
		/// <param name="obj">Object.</param>
		public static object RequireNonNull(object obj)
        {
            if (obj == null)
            {
                throw new NullReferenceException("HPackNullReferenceException");
            }
            return obj;
        }
    }
}
