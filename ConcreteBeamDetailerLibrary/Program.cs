using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcreteBeamDetailerLibrary
{
    public class Program
    {

        public static void Main(string[] args)
        {
            double width = 10;
            double height = 20;
            int stirrup_size = 3;
            double As_req = 3.0;
            int num_layers = 1;
            double side_cover = 1.5;
            double bottom_cover = 1.5;
            double dia_aggreates = 0.75;

            double gap_adjustment = 0.0; 
        }

        /// <summary>
        /// Determines the stirrup / tie bend dia per ACI318 Table 25.3.2
        /// </summary>
        /// <param name="bar_size">rebar size #</param>
        /// <returns>bend dia in inches or -1 if invalid size</returns>
        public static double ACI318_19_StirrupBendDia(double bar_size)
        {
            double bar_dia = bar_size / 8;
            if (bar_size <= 5)
                return 4 * bar_dia;
            else if (bar_size <= 8)
                return 6 * bar_dia;
            else
                return -1;
        }

        public enum MemberTypes
        {
            MEMBER_TYPE_SLAB = 0,
            MEMBER_TYPE_JOIST = 1,
            MEMBER_TYPE_WALL = 2,
            MEMBER_TYPE_BEAM = 3,
            MEMBER_TYPE_COLUMN = 4,
            MEMBER_TYPE_TENSIONTIE = 5,
            MEMBER_TYPE_STRUT = 6,
            MEMBER_TYPE_PEDASTAL = 7,
            MEMBER_TYPE_WALL_BOUNDARYELEMENT = 8
        }

        public enum ReinforcingTypes
        {
            REINF_TYPE_LONGITUDINAL = 0,
            REINF_TYPE_STIRRUP = 1,
            REINF_TYPE_TIE = 2,
            REINF_TYPE_SPIRALS = 3,
            REINF_TYPE_HOOP = 4
        }

        /// <summary>
        /// Computes the minimum cover for CIP members per Table 20.5.1.3.2
        /// </summary>
        /// <param name="bar_size">rebar size #</param>
        /// <param name="is_footing">member is cast against and permanently in contact with ground</param>
        /// <param name="is_exposed_weather">member is exposed to weather or in contact with ground</param>
        /// <param name="mem_type">type of member <see cref="MemberTypes"/></param>
        /// <returns></returns>
        public static double ACI318_19_MinCover_CIP(double bar_size, bool is_footing, bool is_exposed_weather, MemberTypes mem_type, ReinforcingTypes reinf_type, out string msg)
        {
            if (is_footing)
            {
                msg = "3.0 for footings per ACI Table 20.5.1.3.2";
                return 3.0;
            }

            if (is_exposed_weather)
            {
                if(mem_type == MemberTypes.MEMBER_TYPE_SLAB || mem_type == MemberTypes.MEMBER_TYPE_JOIST || mem_type == MemberTypes.MEMBER_TYPE_WALL || mem_type == MemberTypes.MEMBER_TYPE_WALL_BOUNDARYELEMENT)
                {
                    msg = "1.0 for slabs, joists and walls exposed to weather per ACI Table 20.5.1.3.2";
                    return 1.0;
                } else
                {
                    msg = "1.5 for all others exposed to weather per ACI Table 20.5.1.3.2";
                    return 1.5;
                }
            } else
            {
                if (mem_type == MemberTypes.MEMBER_TYPE_SLAB || mem_type == MemberTypes.MEMBER_TYPE_JOIST || mem_type == MemberTypes.MEMBER_TYPE_WALL || mem_type == MemberTypes.MEMBER_TYPE_WALL_BOUNDARYELEMENT)
                {
                    msg = "0.75 for slabs, joists and walls not exposed to weather per ACI Table 20.5.1.3.2";
                    return 0.75;
                } else
                {
                    msg = "0.75 for beams columns and tension ties ";
                    
                    if (reinf_type == ReinforcingTypes.REINF_TYPE_LONGITUDINAL)
                    {
                        msg += "0.75 for primiary reinf. in beams columns and tension ties per ACI Table 20.5.1.3.2";
                        return 1.5;
                    } else
                    {
                        msg += "1.0 for stirrups, ties, and spirals in beams columns and tension ties per ACI Table 20.5.1.3.2";
                        return 1.0;
                    }
                }
            }
        }

        /// <summary>
        /// Returns the minimum bar spacing per Section 25.2
        /// </summary>
        /// <param name="bar_size">rebar size #</param>
        /// <param name="agg_dia">max aggregate size</param>
        /// <returns>minimum horizontal clear spacing</returns>
        public static double ACI318_19_MinHorizontalClearSpacing(double bar_size, double agg_dia, ReinforcingTypes reinf_type, MemberTypes memb_type, out string msg)
        {
            if(reinf_type == ReinforcingTypes.REINF_TYPE_LONGITUDINAL)
            {
                // for longitudinal steel in columns, pedastals, struts, boundary element
                if ((memb_type == MemberTypes.MEMBER_TYPE_COLUMN) || (memb_type == MemberTypes.MEMBER_TYPE_PEDASTAL) || (memb_type == MemberTypes.MEMBER_TYPE_COLUMN) || (memb_type == MemberTypes.MEMBER_TYPE_WALL_BOUNDARYELEMENT))
                {
                        double max = 1.5;
                        max = (1.5 * bar_size / 8.0 > max) ? 1.5 * bar_size / 8.0 : max;
                        max = (max > 1.33 * agg_dia) ? max : 1.33 * agg_dia);
                        msg = "Min. horizontal clear spacing = " + max.ToString() + " per 25.2.3";
                        return max;
                } else
                {
                    double max = 1.0;
                    max = (bar_size / 8.0 > max) ? bar_size / 8.0 : max;
                    max = (max > 1.33 * agg_dia) ? max : 1.33 * agg_dia);
                    msg = "Min. horizontal clear spacing = " + max.ToString() + " per 25.2.1";
                    return max;
                }
            }

            // TODO:: Our catch all case.  Needs further expansion for prestressed and others ACI 25.2.
            double max_default = 1.5;
            max_default = (1.5 * bar_size / 8.0 > max_default) ? 1.5 * bar_size / 8.0 : max_default;
            max_default = (max_default > 1.33 * agg_dia) ? max_default : 1.33 * agg_dia);
            msg = "Min. horizontal clear spacing = " + max_default.ToString() + " per 25.2.3";
            return max_default;
        }

        /// <summary>
        /// Returns the minimum vertical bar spacing per Section 25.2
        /// </summary>
        /// <param name="bar_size">rebar size #</param>
        /// <param name="agg_dia">max aggregate size</param>
        /// <returns>minimum vertical clear spacing</returns>
        public static double ACI318_19_MinVerticalClearSpacing(int num_layers, out string msg)
        {
            msg = "Min. vert. spacing = 1.0 per 25.2.2";
            return 1.0;
        }


        public double Area(double dia)
        {
            return Math.PI * dia * dia / 4;
        } 

    }
}
