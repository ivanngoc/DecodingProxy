using System;

namespace IziHardGames.Libs.Cryptography.Attributes
{
    /// <summary>
    /// Содержит данные о структуре и формате передаваемых значений
    /// </summary>
    public class Map : Attribute
    {
        public string Title { get; set; }

        public Map(string title)
        {
            this.Title = title;
        }
    }
}
