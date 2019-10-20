using System.ComponentModel;

namespace EASEncoder.Models.SAME
{
    public class SAMERegion
    {
        [DisplayName("County Name")]
        public string CountyName
        {

            get
            {
                if (State.Id != 0 && County.Id == 0)
                {
                    return County.Name;
                }
                else if (State.Id == 0)
                {
                    return "All of United States";
                }
                else
                {
                    return County.Name + ", " + State.Name;
                }
            }
        }

        [DisplayName("State Name")]
        public string StateName
        {

            get
            {
                if (State.Id == 0)
                {
                    return "All of United States";
                }
                else
                {
                    return State.Name;
                }
            }
        }

        public SAMECounty County;
        public SAMEState State;
        public SAMESubdivision Subdivision = new SAMESubdivision(0, "Entire Region");

        public SAMERegion(SAMEState state, SAMECounty county)
        {
            State = state;
            County = county;
        }
    }
}