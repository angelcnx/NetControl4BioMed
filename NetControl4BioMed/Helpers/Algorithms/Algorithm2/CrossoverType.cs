﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Algorithms.Algorithm2
{
    /// <summary>
    /// Represents the possible crossover algorithms for an analysis.
    /// </summary>
    public enum CrossoverType
    {
        /// <summary>
        /// Represents the default, standard, crossover algorithm.
        /// </summary>
        [Display(Name = "Weighted random parent", Description = "The default, standard, crossover algorithm.")]
        WeightedRandom,

        /// <summary>
        /// Represents the default, standard, crossover algorithm that is twice more likely to choose preferred nodes.
        /// </summary>
        [Display(Name = "Weighted random preferred parent", Description = "The default, standard, crossover algorithm that is twice more likely to choose preferred nodes.")]
        WeightedRandomWithPreference,

        /// <summary>
        /// Represents the previously used crossover algorithm.
        /// </summary>
        [Display(Name = "Dominant parent", Description = "The previously used crossover algorithm.")]
        Dominant,

        /// <summary>
        /// Represents the previously used crossover algorithm that chooses preferred nodes whenever possible.
        /// </summary>
        [Display(Name = "Dominant preferred parent", Description = "The previously used crossover algorithm that chooses preferred nodes whenever possible.")]
        DominantWithPreference
    }
}
