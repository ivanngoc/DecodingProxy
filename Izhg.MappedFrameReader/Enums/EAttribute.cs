using System.Linq;
using IziHardGames.MappedFrameReader;
using static IziHardGames.MappedFrameReader.SchemeImporter;
using Constants = IziHardGames.MappedFrameReader.ConstantsForMappedReader;

namespace IziHardGames.MappedFrameReader
{
    internal enum EAttribute
    {
        None,
        Size,
        LengthType,
        SourceType,
        /// <summary>
        /// ID. Defined Name as StringID
        /// </summary>
        Name,
        /// <summary>
        /// ID of Source
        /// </summary>
        SourceName,
        /// <summary>
        /// Read Mode - Repeat
        /// </summary>
        Repeat,
        ConditionType,
        ConditionValue,
        /// <summary>
        /// Advancing algoritm
        /// </summary>
        AdvanceMode,
        AsignedValue,
        Type,
        Separator,
        /// <summary>
        /// Sequence To indicate the end of repeation if there is no explicitly specified repeating count
        /// </summary>
        Enclose,
        /// <summary>
        /// Cast to specified task with specified length
        /// </summary>
        Cast,
    }

    internal static class AttributesUtility
    {
        internal static void ValidateForRepeat(SchemeImporter.Attribute[] atrs)
        {
            //Incompatible
            if (atrs.Any(x => x.type == EAttribute.ConditionValue) && atrs.Any(x => x.type == EAttribute.Separator))
                throw new FormatException($"Incompatible atrs. ConditionValue and Separator");

            //Linked
            var isSeparator = atrs.Any(x => x.type == EAttribute.Separator);
            var isEnclose = atrs.Any(x => x.type == EAttribute.Enclose);
            var conditionType = atrs.Any(x => x.type == EAttribute.ConditionType);
            var conditionValue = atrs.Any(x => x.type == EAttribute.ConditionValue);
            var isAlgoFunc = atrs.Any(x => x.type == EAttribute.ConditionValue && x.valueString == Constants.ATR_ALGO_FUNC);

            // must be together if any presented
            if (conditionType)
            {
                if (isSeparator ^ isEnclose) throw new System.FormatException("Separator And Enclose must be specified together");
            }

            if (isAlgoFunc && (isAlgoFunc ^ conditionValue)) throw new FormatException("Algo Func and ConditionValue Must be specified together");
            // must have
            //if (!atrs.Any(x => x.type == EAttribute.Repeat)) throw new System.FormatException("Repeat must be presented");
        }
    }
}