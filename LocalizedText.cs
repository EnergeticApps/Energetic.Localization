using Energetic.People.ValueObjects;
using Energetic.Globalization.ValueObjects;

namespace Energetic.Localization
{
    internal class LocalizedText : LocalizedText<string>
    {
        public LocalizedText(string text, UserId? translatedByUserId = null, Culture? translatedFrom = null) : 
            base(text, translatedByUserId, translatedFrom)
        {
        }
    }

    internal class LocalizedText<T>
    {
        internal LocalizedText(T text, UserId? translatedByUserId = null, Culture? translatedFrom = null)
        {
            TranslatedByUserId = translatedByUserId;
            TranslatedFrom = translatedFrom;
            Text = text;
            IsObsolete = false;
        }

        public T Text { get; private set; }

        public Culture? TranslatedFrom { get; private set; }

        public bool IsObsolete { get; private set; }

        public UserId? TranslatedByUserId { get; private set; }

        public void MakeObsolete()
        {
            IsObsolete = true;
        }
    }
}