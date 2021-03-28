using Energetic.Globalization.ValueObjects;
using Energetic.Localization;
using Energetic.People.ValueObjects;
using System;
using System.Collections.Generic;

namespace Energetic.Text.Localization
{ 
    /// <summary>
    /// Manages text that can be localized. Use this class instead of <see cref="string"/> (or instead of any value object wrapper of
    /// <see cref="string"/>) if you want to localize it. Allows translations of the text to be added, edited and removed.
    /// </summary>
    public class LocalizableText : LocalizableText<string>
    {
        /// <summary>
        /// Constructor for this class
        /// </summary>
        /// <param name="originalCulture"></param>
        /// <param name="original"></param>
        /// <param name="maintainMaterialIntegrity">Specify a value of <see cref="true" /> if your translations need to be updated whenever
        /// the meaning of the original text gets changed. Specify <see cref="false"/> if it's ok for your translations to vary materially from
        /// the original text. Immaterial changes (i.e. spelling corrections and stylistic improvements) will always be ignored by this feature.</param>
        public LocalizableText(Culture originalCulture,
                                   string original,
                                   bool maintainMaterialIntegrity) : base(originalCulture, original, maintainMaterialIntegrity)
        {
        }
    }

    public class LocalizableText<TProperty>
    {
        private IDictionary<Culture, LocalizedText<TProperty>> _localVersions = new Dictionary<Culture, LocalizedText<TProperty>>();

        /// <summary>
        /// Constructor for this class
        /// </summary>
        /// <param name="originalCulture"></param>
        /// <param name="original"></param>
        /// <param name="maintainMaterialIntegrity">Specify a value of <see cref="true" /> if your translations need to be updated whenever
        /// the meaning of the original text gets changed. Specify <see cref="false"/> if it's ok for your translations to vary materially from
        /// the original text. Immaterial changes (i.e. spelling corrections and stylistic improvements) will always be ignored by this feature.</param>
        public LocalizableText(Culture originalCulture,
                                   TProperty original,
                                   bool maintainMaterialIntegrity)
        {
            if (originalCulture is null)
                throw new ArgumentNullException(nameof(originalCulture));

            if (original is null)
                throw new ArgumentNullException(nameof(original));

            MaintainMaterialIntegrity = maintainMaterialIntegrity;
            OriginalCulture = originalCulture;

            _localVersions.Add(originalCulture, new LocalizedText<TProperty>(original));
        }

        /// <summary>
        /// Specifies whether or not this <see cref="LocalizedDictionary{TAggregate, TProperty}"/> should mark translations as obsolete when
        /// the text from which they were translated changes materially in meaning.
        /// </summary>
        /// <remarks>A value of <see cref="true" /> means that translations will be marked as obsolete whenever
        /// the meaning of the original text gets changed. A value of <see cref="false"/> means that it's ok for translations to vary materially from
        /// the original text. Immaterial changes (i.e. spelling corrections and stylistic improvements) will always be ignored by this feature.</remarks>
        public bool MaintainMaterialIntegrity { get; private set; }

        public Culture OriginalCulture { get; private set; }

        public void AddTranslation(Culture culture, TProperty text, UserId translatedByUserId, Culture translatedFrom)
        {
            if (culture is null)
                throw new ArgumentNullException(nameof(culture));

            if (text is null)
                throw new ArgumentNullException(nameof(text));

            if (translatedByUserId is null)
                throw new ArgumentNullException(nameof(translatedByUserId));

            if (translatedFrom is null)
                throw new ArgumentNullException(nameof(translatedFrom));

            _localVersions.Add(culture, new LocalizedText<TProperty>(text, translatedByUserId, translatedFrom));
        }

        public void RemoveTranslation(Culture culture, bool removeAllLocalVersionsTranslatedFromIt)
        {
            if (culture is null)
                throw new ArgumentNullException(nameof(culture));

            _localVersions.Remove(culture);

            if (removeAllLocalVersionsTranslatedFromIt)
            {
                RemoveAllLocalVersionsTranslatedFrom(culture);
            }
        }

        public void UpdateOriginal(TProperty text, bool isMaterial)
        {
            if (text is null)
                throw new ArgumentNullException(nameof(text));

            _localVersions[OriginalCulture] = new LocalizedText<TProperty>(text);

            if (MaintainMaterialIntegrity && isMaterial)
            {
                MakeLocalVersionsObsoleteIfTranslatedFrom(OriginalCulture);
            }
        }

        public void UpdateTranslation(TProperty text, Culture culture, UserId translatedByUserId, Culture translatedFrom, bool isMaterial)
        {
            if (text is null)
                throw new ArgumentNullException(nameof(text));

            if (culture is null)
                throw new ArgumentNullException(nameof(culture));

            if (translatedByUserId is null)
                throw new ArgumentNullException(nameof(translatedByUserId));

            if (translatedFrom is null)
                throw new ArgumentNullException(nameof(translatedFrom));

            _localVersions[culture] = new LocalizedText<TProperty>(text, translatedByUserId, translatedFrom);

            if (MaintainMaterialIntegrity && isMaterial)
            {
                MakeLocalVersionsObsoleteIfTranslatedFrom(culture);
            }
        }
        private void MakeLocalVersionsObsoleteIfTranslatedFrom(Culture translatedFrom)
        {
            foreach (var localVersion in _localVersions)
            {
                var localVersionCulture = localVersion.Key;
                var translation = localVersion.Value;

                if (translation.TranslatedFrom == translatedFrom)
                {
                    translation.MakeObsolete();
                    MakeLocalVersionsObsoleteIfTranslatedFrom(localVersionCulture);
                }
            }
        }

        private void RemoveAllLocalVersionsTranslatedFrom(Culture culture)
        {
            foreach (var localVersion in _localVersions)
            {
                var localVersionCulture = localVersion.Key;
                var translation = localVersion.Value;

                if (translation.TranslatedFrom == culture)
                {
                    RemoveTranslation(localVersionCulture, true);
                }
            }
        }
    }
}