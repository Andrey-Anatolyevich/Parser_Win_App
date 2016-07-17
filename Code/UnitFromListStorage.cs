using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IParserPlugin;

namespace Code {
    class DBEmulator {
        private List<UnitFromList> Units;
        private List<UnitDetails> UnitDetails;

        public DBEmulator() {
            this.Units = new List<UnitFromList>();
            this.UnitDetails = new List<UnitDetails>();
        }
        public void SaveUnitsList(List<UnitFromList> input) {
            bool Added = false;
            string ParentUrl = "";
            foreach(UnitFromList U in input) {
                if(!Units.Any(x => x.Url == U.Url)) {
                    Units.Add(U);
                    Added = true;
                    ParentUrl = U.UrlParent;
                }
            }
            // обрежем лишние элементы, чтобы освободить память.
            if (Added == true && Units.Count > 300) {
                Units = Units.Where(x => x.UrlParent == ParentUrl).OrderByDescending(x => x.FoundDT).Take(300).ToList();
            }
        }
        public void SaveUnitsDetailsList(List<UnitDetails> input) {
            bool Added = false;
            string ParentUrl = "";
            foreach (UnitDetails U in input) {
                Units.Where(w => w.IsParsed == false
                    && UnitDetails.Any(x=> x.AddID == w.AddID)).ToList().ForEach(i => i.IsParsed = true);
                if (!UnitDetails.Any(x => x.Url == U.Url)) {
                    UnitDetails.Add(U);
                    Added = true;
                    ParentUrl = U.UrlParent;
                }
            }
            // обрежем лишние элементы, чтобы освободить память.
            if (input != null) {
                // если по нужному нам parentURL получено больше 500 элементов, то отрежем лишнее.
                if (Added == true && UnitDetails.Where(x => x.UrlParent == input.FirstOrDefault().UrlParent).ToList().Count > 500) {
                    UnitDetails = UnitDetails.Where(x => x.UrlParent == ParentUrl).OrderByDescending(x => x.TimeAdded).Take(500).ToList();
                }
            }
        }
        public List<UnitFromList> GetUnparsedListUnits(int priceMin, int priceMax) {
            List<UnitFromList> Result = Units.Where(x => x.IsParsed == false
                && x.Price >= priceMin).ToList();
            if(priceMax != 0) {
                Result = Result.Where(x => x.Price <= priceMax).ToList();
            }
            return Result;
        }
        public List<UnitDetails> GetUnshipped(int priceMin, int priceMax, string urlOriginal, 
            List<string> constraints, List<string> keyWords, bool OnlyWithPictures) {
            List<UnitDetails> AllList = UnitDetails.Where(x =>
				x.EMailed == false
                && x.Price >= priceMin
                && x.UrlParent == urlOriginal
               ).ToList();
            if (priceMax > 0) {
                AllList = AllList.Where(x =>
                    x.Price <= priceMax).ToList();
            }
            List<UnitDetails> Result = AllList;
            if (constraints.Count > 0) {
                List<UnitDetails> BadUnits = UnitDetails.Where(x =>
                    constraints.Any(y =>
                        (x.AddText + " " + x.Title).ToLower().IndexOf(y.ToLower()) >= 0)
                        && x.UrlParent == urlOriginal
                        ).ToList();
                Result = AllList.Except(BadUnits).ToList();
            }
            if (keyWords.Count > 0) {
                Result = Result.Where(x => keyWords.Any(y =>
                    (x.AddText + " " + x.Title).ToLower().IndexOf(y.ToLower()) >= 0)
                    && x.UrlParent == urlOriginal
                    ).ToList();
            }
            if(OnlyWithPictures) {
                Result = Result.Where(x => x.Pictures.Count > 0).ToList();
            }
            return Result;
        }
		public void SetShipped(Set.Interest intake, string urlOriginal) {
            List<UnitDetails> Result = GetUnshipped(intake.PriceMin,
                    intake.PriceMax, urlOriginal, intake.ConstraintsList, intake.KeyWords, intake.OnlyWithPictures);
            foreach (UnitDetails D in Result) {
                UnitDetails.Where(x => x.AddID == D.AddID).ToList().ForEach(n => n.EMailed = true);
            }
		}
		public List<UnitDetails> GetShippedTotal() {
			List<UnitDetails> Result = UnitDetails.Where(x => 
				x.EMailed == true).ToList();
			return Result;
		}
        public List<UnitDetails> GetAllUnitDetails() {
            int HowMuchToTake = 300;
            if (UnitDetails.Count < 300)
                HowMuchToTake = UnitDetails.Count;
            return UnitDetails.OrderByDescending(x => x.TimeAdded).Take(HowMuchToTake).ToList();

        }
    }
}
