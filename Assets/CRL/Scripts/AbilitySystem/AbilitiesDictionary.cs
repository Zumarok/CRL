using System;
using System.Collections.Generic;

namespace Crux.CRL.AbilitySystem
{
    public class AbilitiesDictionary : Dictionary<AbilityName, Ability>
    {
        /// <summary>
        /// Returns a reference to an ability's data.
        /// </summary>
        public Ability Ability(AbilityName abilityName)
        {
            if (TryGetValue(abilityName, out var a))
                return a;

            throw new Exception($"'{abilityName}' does not exist in the AbilitiesDictionary.");
        }
    }
}
