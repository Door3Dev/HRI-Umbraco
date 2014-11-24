using System.Collections.Generic;

namespace HRI.Models
{
    public class ProductsData
    {
        #region Regions Counties
        public Dictionary<string, int> Regions = new Dictionary<string, int>() { 
            {"Warren", 1},
            {"Washington", 1},
            {"Rensselaer", 1},
            {"Columbia", 1},
            {"Greene", 1},
            {"Schoharie", 1},
            {"Montgomery", 1},
            {"Fulton", 1},
            {"Saratoga", 1},
            {"Schenectady", 1},
            {"Albany", 1},
            {"Orleans", 2},
            {"Genesee", 2},
            {"Wyoming", 2},
            {"Allegany", 2},
            {"Cattaraugus", 2},
            {"Chautauqua", 2},
            {"Erie", 2},
            {"Niagara", 2},
            {"Delaware", 3},
            {"Ulster", 3},
            {"Dutchess", 3},
            {"Putnam", 3},
            {"Orange", 3},
            {"Sullivan", 3},
            {"Westchester", 4},
            {"Rockland", 4},
            {"New York", 4},
            {"Kings", 4},
            {"Queens", 4},
            {"Richmond", 4},
            {"Bronx", 4},
            {"Monroe", 5},
            {"Wayne", 5},
            {"Seneca", 5},
            {"Yates", 5},
            {"Livington", 5},
            {"Ontario", 5},
            {"Onondaga", 6},
            {"Cortland", 6},
            {"Broome", 6},
            {"Tioga", 6},
            {"Chemug", 6},
            {"Steuben", 6},
            {"Schuyler", 6},
            {"Tompkins", 6},
            {"Cayuga", 6},
            {"Clinton", 7},
            {"Franklin", 7},
            {"Essex", 7},
            {"Hamilton", 7},
            {"Herkimer", 7},
            {"Otsego", 7},
            {"Chenango", 7},
            {"Madison", 7},
            {"Oneida", 7},
            {"Lewis", 7},
            {"Oswego", 7},
            {"Jefferson", 7},
            {"St. Lawrence", 7},
            {"Nassau", 8},
            {"Suffolk", 8}
        };
        #endregion
        #region Base Rates
        public double BaseRate = 322.73;
        public Dictionary<int, double> RegionsFactor = new Dictionary<int, double>
        {
            {1, 0.8408},
            {2, 0.7483},
            {3, 0.9445},
            {4, 1.0865},
            {5, 0.7737},
            {6, 0.7232},
            {7, 0.7950},
            {8, 1.0865}
        };
        public double ConversionFactor = 1.00;
        public double IndividualFactor = 1.00;
        public double CoupleFactor = 2.00;
        public double PrimarySubscriberAnd1DependentFactor = 1.70;
        public double PrimarySubscriberAnd2DependentFactor = 1.70;
        public double PrimarySubscriberAnd3DependentFactor = 1.70;
        public double CoupleAnd1DependentFactor = 2.85;
        public double CoupleAnd2DependentFactor = 2.85;
        public double CoupleAnd3DependentFactor = 2.85;
        public double ChildOnly = 0.412;
        #endregion

        #region Products Data
        public Dictionary<string, Product> Products = new Dictionary<string, Product>() {
            { 
                "EssentialCare",
                new Product {
                    Name = "EssentialCare Plan",
                    Description = "EssentialCare is our standard plan offering, aligning with state and federal requirements for deductibles, copays, and other coverage benefits, allowing consumers to compare EssentialCare “apples to apples” with plans from other insurers.",
                    Plans = new List<Plan> {
                        new Plan {
                            HiosId = "71644NY0010004-00",
                            MetalTier = "Platinum",
                            RateFactor = 1.6795,
                            Description = PlansDescriptioData.EssentialCarePlatinumPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0010003-00",
                            MetalTier = "Gold",
                            RateFactor = 1.4284,
                            Description = PlansDescriptioData.EssentialCareGoldPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0010002-00",
                            MetalTier = "Silver",
                            RateFactor = 1.2224,
                            Description = PlansDescriptioData.EssentialCareSilverPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0010001-00",
                            MetalTier = "Bronze",
                            RateFactor = 1.000000,
                            Description = PlansDescriptioData.EssentialCareBronzePlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0010005-00",
                            MetalTier = "Catastrophic",
                            RateFactor = 0.5253,
                            Description = PlansDescriptioData.EssentialCareCatastrophicPlanDescription
                        }
                    }
                }
            },
             { 
                "TotalIndependence",
                new Product {
                    Name = "TotalIndependence Plan",
                    Description = "This low premium, high deductible plan is a basic option for individuals.",
                    Plans = new List<Plan> {
                        new Plan {
                            HiosId = "71644NY0730001-00",
                            MetalTier = "Gold",
                            RateFactor = 1.2787,
                            Description = PlansDescriptioData.TotalIndependenceGoldPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0700001-00",
                            MetalTier = "Silver",
                            RateFactor = 1.0838,
                            Description = PlansDescriptioData.TotalIndependenceSilverPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0670001-00",
                            MetalTier = "Bronze",
                            RateFactor = 0.9514,
                            Description = PlansDescriptioData.TotalIndependenceBronzePlanDescription
                        }
                    }
                }
            },
            { 
                "PrimarySelect", 
                new Product {
                    Name = "PrimarySelect Plan",
                    Description = "PrimarySelect is Health Republic’s signature program, emphasizing the role of a primary care physician in our members’ health. After selecting a primary care physician, visits are always free of charge. This promotes preventive care and wellness screenings, which leads to better health outcomes for patients and lower costs for everyone.",
                    Plans = new List<Plan> {
                        new Plan {
                            HiosId = "71644NY0030004-00",
                            MetalTier = "Platinum",
                            RateFactor = 1.6101,
                            Description = PlansDescriptioData.PrimarySelectPlatinumPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0030003-00",
                            MetalTier = "Gold",
                            RateFactor = 1.4273,
                            Description = PlansDescriptioData.PrimarySelectGoldPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0030002-00",
                            MetalTier = "Silver",
                            RateFactor = 1.2216,
                            Description = PlansDescriptioData.PrimarySelectSilverPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0030001-00",
                            MetalTier = "Bronze",
                            RateFactor = 1.1218,
                            Description = PlansDescriptioData.PrimarySelectBronzePlanDescription
                        },
                    }
                }
            },

            { 
                "PrimarySelectPCMH", 
                new Product {
                    Name = "PrimarySelect PCMH Plan",
                    Description = "Similar in structure to PrimarySelect, PrimarySelect PCMH (Patient Centered Medical Homes) focuses on comprehensive patient care with a specialized network of patient centered medical homes certified by the National Committee for Quality Assurance (NCQA). Only available at the Silver level, PrimarySelect PCMH is a cost-friendly option for those looking to get the most out of their health plan.",
                    Plans = new List<Plan> {
                        new Plan {
                            HiosId = "71644NY0040002-00",
                            MetalTier = "Silver",
                            RateFactor = 1.1526,
                            Description = PlansDescriptioData.PrimarySelectSilverPCMHPlanDescription
                        }
                    }
                }
            },

            { 
                "EssentialCareChildOnly", 
                new Product {
                    Name = "EssentialCare Child Only Plan",
                    Plans = new List<Plan> {
                        new Plan {
                            HiosId = "71644NY0020004-00",
                            MetalTier = "Platinum",
                            RateFactor = 1.6795,
                            Description = PlansDescriptioData.EssentialCarePlatinumPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0020003-00",
                            MetalTier = "Gold",
                            RateFactor = 1.4284,
                            Description = PlansDescriptioData.EssentialCareGoldPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0020002-00",
                            MetalTier = "Silver",
                            RateFactor = 1.2224,
                            Description = PlansDescriptioData.EssentialCareSilverPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0020001-00",
                            MetalTier = "Bronze",
                            RateFactor = 1.0000,
                            Description = PlansDescriptioData.EssentialCareBronzePlanDescription
                        }
                    }
                }
            },

              { 
                "TotalIndependenceChildOnly", 
                new Product {
                    Name = "TotalIndependence Child Only Plan",
                    Plans = new List<Plan> {
                        new Plan {
                            HiosId = "71644NY0740001-00",
                            MetalTier = "Gold",
                            RateFactor = 1.2787,
                            Description = PlansDescriptioData.TotalIndependenceGoldPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0710001-00",
                            MetalTier = "Silver",
                            RateFactor = 1.0838,
                            Description = PlansDescriptioData.TotalIndependenceSilverPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0680001-00",
                            MetalTier = "Bronze",
                            RateFactor = 0.9514,
                            Description = PlansDescriptioData.TotalIndependenceBronzePlanDescription
                        }
                    }
                }
            },

            { 
                "EssentialCare29", 
                new Product {
                    Name = "EssentialCare Plan 29",
                    Plans = new List<Plan> {
                        new Plan {
                            HiosId = "71644NY0090004-00",
                            MetalTier = "Platinum",
                            RateFactor = 1.6795,
                            Description = PlansDescriptioData.EssentialCarePlatinumPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0090003-00",
                            MetalTier = "Gold",
                            RateFactor = 1.4284,
                            Description = PlansDescriptioData.EssentialCareGoldPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0090002-00",
                            MetalTier = "Silver",
                            RateFactor = 1.2224,
                            Description = PlansDescriptioData.EssentialCareSilverPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0090001-00",
                            MetalTier = "Bronze",
                            RateFactor = 1.000000,
                            Description = PlansDescriptioData.EssentialCareBronzePlanDescription
                        }
                    }
                }
            },

             { 
                "TotalIndependence29", 
                new Product {
                    Name = "TotalIndependence Plan 29",
                    Plans = new List<Plan> {
                        new Plan {
                            HiosId = "71644NY0850001-00",
                            MetalTier = "Platinum",
                            RateFactor = 1.6314,
                            Description = PlansDescriptioData.TotalIndependencePlatinumPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0750001-00",
                            MetalTier = "Gold",
                            RateFactor = 1.2787,
                            Description = PlansDescriptioData.TotalIndependenceGoldPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0720001-00",
                            MetalTier = "Silver",
                            RateFactor = 1.0838,
                            Description = PlansDescriptioData.TotalIndependenceSilverPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0690001-00",
                            MetalTier = "Bronze",
                            RateFactor = 0.9514,
                            Description = PlansDescriptioData.TotalIndependenceBronzePlanDescription
                        }
                    }
                }
            },

            { 
                "PrimarySelect29", 
                new Product {
                    Name = "PrimarySelect Plan 29",
                    Plans = new List<Plan> {
                        new Plan {
                            HiosId = "71644NY0130004-00",
                            MetalTier = "Platinum",
                            RateFactor = 1.6101,
                            Description = PlansDescriptioData.PrimarySelectPlatinumPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0130003-00",
                            MetalTier = "Gold",
                            RateFactor = 1.4273,
                            Description = PlansDescriptioData.PrimarySelectGoldPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0130002-00",
                            MetalTier = "Silver",
                            RateFactor = 1.2216,
                            Description = PlansDescriptioData.PrimarySelectSilverPlanDescription
                        },
                        new Plan {
                            HiosId = "71644NY0130001-00",
                            MetalTier = "Bronze",
                            RateFactor = 1.1218,
                            Description = PlansDescriptioData.PrimarySelectBronzePlanDescription
                        }
                    }
                }
            },

            { 
                "PrimarySelectPCMH29", 
                new Product {
                    Name = "PrimarySelect PCMH Plan 29",
                    Plans = new List<Plan> {
                        new Plan {
                            HiosId = "71644NY0150002-00",
                            MetalTier = "Silver",
                            RateFactor = 1.1526,
                            Description = PlansDescriptioData.PrimarySelectSilverPCMHPlanDescription
                        }
                    }
                }
            }
        };
        #endregion

    }
}