﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Algorithms.Algorithm2
{
    /// <summary>
    /// Represents the possible mutation algorithms for an analysis.
    /// </summary>
    public enum MutationType
    {
        /// <summary>
        /// Represents the default, standard, mutation algorithm.
        /// </summary>
        [Display(Name = "Weighted random ancestor")]
        WeightedRandomAncestor,

        /// <summary>
        /// Represents the default, standard, mutation algorithm that is twice less likely to mutate preferred nodes.
        /// </summary>
        [Display(Name = "Weighted random preferred ancestor")]
        WeightedRandomAncestorWithPreference,

        /// <summary>
        /// Represents the previously used mutation algorithm.
        /// </summary>
        [Display(Name = "Random ancestor")]
        RandomAncestor,

        /// <summary>
        /// Represents the previously used mutation algorithm that mutates into preferred nodes whenever possible.
        /// </summary>
        [Display(Name = "Random preferred ancestor")]
        RandomAncestorWithPreference
    }
}
