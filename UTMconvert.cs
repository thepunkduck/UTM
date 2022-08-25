using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;





/*
 * see:
 * http://cholla.mmto.org/gtopo/reference/utm/convert.html
 * https://www.dmap.co.uk/utmworld.htm
 * 
 * check your values with:
 * http://rcn.montana.edu/resources/Converter.aspx
 * 
 * 
 * GDA94 and WGS84 are very similar:
 * Read:"What is the difference between GDA94 and WGS84?" on 
 * http://www.geoproject.com.au/gda.faq.html#q09
 * 
 * 
 * 
 * 
 * static public void Test()
 * just a test 
 * 
 * 
 * ZONE QUERY METHODS ---------------------------------------------------------
 * 
 *  public static int GetZoneNumber(double lat, double @long)
 *      get the zone number from the logitude (with mods for Svalbard/Norway area)
 *  
 *  
 *  public static int GetBestZoneNumber(double[] latLong)
 *      similar to GetZoneNumber(), but takes the most frequent zone from a list of lat/longs
 *  
 *  
 *  
 *  MAIN CONVERSION METHODS --------------------------------------------------
 *  
 *  public static void LatLong_to_UTM(GLOBALDATUM globaldatum,
            double lat, double @long,
            int fixedZoneNumber, // zero is automatic
            HEMISPHERE preferredHemisphere,
            out double utmEasting,
            out double utmNorthing,
            out string zoneStr, out int zoneNumber, out char zoneLetter)
 *      from lat/long and projection and zone info, get the easting, northing and zone values
 * 
 * 
 * 
 * public static void UTM_to_LatLong(GLOBALDATUM globaldatum,
            int zoneNumber, char zoneLetter,
            double utmEasting,
            double utmNorthing,
            out double latitude, out double longitude,
            out HEMISPHERE hemisphere)
 *      from easting/northing etc, get the lat/long
 * 
 * 
 * 
 *  UTILITY METHODS ----------------------------------------------------------
 * 
 * public static char[] ValidZoneLetters
 *      a list of the valid zone charaters
 * 
 * 
 * public static bool IsInZoneRange(int zone, int zoneMin, int zoneMax)
 *      determine if zone is in a zone range. Takes account of wrap around through the Pacific.
 *      good if you live in Fiji
 *      
 *      
 * public static char UTMLetterDesignator(double lat)
 *      Letters designated to latitude bands (eg England in U, Mexico in Q, Antartica in D and C)
 * 
 *  
 * public static GridSquare[] GetAdjacentGridSquares(int zoneNum, char zoneLetter, int layer = 1)
 *      Get the adjacent grid squares to supplied zoneNum/zoneLetter 
 *      layers is how many layers out to search. 
 *      
 * public static GridSquare[] GetAdjacentGridSquares(int zoneNum, char zoneLetter, int hW, int hH)
 *      Get the adjacent grid squares to supplied zoneNum/zoneLetter 
 *      hW and hH determines amounts above and below
 *      
 * public static void GetLatLongRange(int zoneNumber, char letter, out double lat0, out double lat1, out double lon0, out double lon1)
 *      Get the lat/long limits of a particular square
 * 
 * 
 * 
 */












namespace SmashTheState
{
    public static class UTM
    {
        private static List<CoordSysDefinition> _sysList;
        private const double deg2rad = Math.PI / 180;
        private const double rad2deg = 180.0 / Math.PI;

        public static int[] ValidZoneNumbers;

        static UTM()
        {
            // initialize projection systems available 
            _sysList = new List<CoordSysDefinition>
            {
                new CoordSysDefinition(GLOBALDATUM.WGS84, 6378137, 298.257223563),
                new CoordSysDefinition(GLOBALDATUM.WGS66, 6378145.0, 298.25),
                new CoordSysDefinition(GLOBALDATUM.WGS72, 6378135.0, 298.26),
                new CoordSysDefinition(GLOBALDATUM.GRS80, 6378137, 298.2572236),
                new CoordSysDefinition(GLOBALDATUM.AGD66, 6378160.000, 298.25),
                new CoordSysDefinition(GLOBALDATUM.AGD84, 6378160.000, 298.25),
                new CoordSysDefinition(GLOBALDATUM.GDA94, 6378137, 298.2572221),
                new CoordSysDefinition(GLOBALDATUM.GDA2000, 6378137, 298.2572221)
            };

            ValidZoneNumbers = new int[60];
            for (var i = 0; i < 60; i++) ValidZoneNumbers[i] = i + 1;
        }




        // usage:
        static public void Test()
        {
            // see: https://www.dmap.co.uk/utmworld.htm
                    
                    
                    
          
            // lat longs of us cities
            double[] latLongs = {
                    42.203217,-72.625481,
                    42.587334,-72.603416,
                    42.280418,-71.423233,
                    42.586716,-71.814468,
                    42.408623,-71.056999,
                    42.392925,-71.037109,
                    42.856842,-70.96344 ,
                    38.981544,-77.010674,
                    38.36335 ,-75.605919,
                    39.086437,-77.161263,
                    39.644207,-77.73143 ,
                    38.998318,-76.896332,
                    39.649109,-78.769714,
                    38.563461,-76.085251,
                    39.514877,-76.17411 ,
                    46.680672,-68.023521,
                    43.680031,-70.310425,
                    44.101902,-70.21711 ,
                    44.230553,-69.779633,
                    44.90691 ,-66.996201,
                    45.188042,-67.282753,
                    43.489849,-70.469711,
                    44.42477 ,-69.01062 ,
                    43.917503,-69.829712,
                    44.331493,-69.788994,
                    44.090008,-70.271439,
                    29.795111,-90.82814 ,
                    30.124033,-91.833435,
                    29.706043,-91.206917,
                    32.50993 ,-92.121742,
                    29.916653,-90.057854,
                    32.778889,-91.919243,
                    31.284788,-92.471176,
                    37.74688 ,-84.30146 ,
                    38.206348,-84.270172,
                    37.760586,-87.127686,
                    39.08897 ,-84.500786,
                    36.616894,-83.739494,
                    36.739876,-88.646523,
                    37.569702,-85.743095,
                    37.250626,-83.195503,
                    37.76627 ,-84.852119,
                    36.843681,-83.324593,
                    38.192902,-84.883942,
                    37.703224,-85.871674,
                    39.067719,-84.516426,
                    37.344158,-85.346436,
                    36.976524,-86.456017,
                    37.569199,-84.299782,
                    37.820415,-85.467949,
                    36.869038,-83.892479,
                    38.460304,-82.649666,
                    39.779823,-98.787064,
                    38.826633,-97.616257,
                    37.406769,-94.705528,
                    38.604465,-95.271301,
                    38.497746,-94.960602,
                    38.047016,-97.350304,
                    39.313015,-94.941147,
                    38.960213,-95.27739 ,
                    39.026646,-96.852814,
                    38.88509 ,-99.326202,
                    38.364895,-98.7743  ,
                    37.838108,-94.710022,
                    38.405903,-96.188339,
                    37.753098,-100.024872  ,
                    38.661697,-96.492599,
                    37.679878,-95.459778,
                    39.563606,-95.125549,
                    41.57983 ,-93.791328,
                    42.464397,-93.829056,
                    42.499504,-92.358665,
                    42.495132,-96.40007 ,
                    41.016621,-92.43055 ,
                    41.299023,-92.653198,
                    41.703957,-93.054817,
                    40.969242,-91.55542 ,
                    40.407204,-91.410805,
                    41.746952,-92.729362,
                    42.512745,-94.188148,
                    43.4035  ,-94.843323,
                    42.516033,-90.718506,
                    41.250854,-95.882042,
                    41.827965,-90.249619,
                    42.754681,-95.557831,
                    43.066807,-92.683464,
                    40.810947,-91.131844,
                    42.06065 ,-93.88549 ,
                    40.800262,-85.830887,
                    38.678299,-87.522491,
                    41.483845,-87.063965,
                    39.472298,-87.401917,
                    39.524483,-85.786476,
                    39.828354,-84.894196,
                    41.345619,-86.316971,
                    40.754417,-86.071426,
                    39.924065,-85.379021,
                    38.313908,-85.835434,
                    41.66853 ,-86.173042,
                    40.55648 ,-85.68277};

            var bestUSzone = GetBestZoneNumber(latLongs);
                       
                    
               
                    
                    
                    
            // melbourne : 37.8136° S, 144.9631° E

            // with fixed zone: (works out to be 55)
            // look at a map or use this method
            var melb_lat = -37.8136;
            var melb_long = 144.9631;
            var requiredZoneNumber = UTM.GetZoneNumber(melb_lat, melb_long);
            var preferredHemisphere = HEMISPHERE.Southern;
            UTM.LatLong_to_UTM(GLOBALDATUM.GDA94,
                melb_lat, melb_long,
                requiredZoneNumber,
                preferredHemisphere,
                                    out var easting,
                                    out var northing,
                                    out var zoneStr, out var zoneNum, out var zoneLetter);

            Console.WriteLine($"FIXED:");
            Console.WriteLine($"easting:{easting:F2}, northing:{northing:F2}");
            Console.WriteLine($"zone: {zoneNum} letter:{zoneLetter}");

            // all automatic
            // note, that with a map, you'll want to ensure that zone is fixed
            // or you'll get discontinuities if lines pass over boundaries of zone
            // 
            UTM.LatLong_to_UTM(GLOBALDATUM.GDA94,
                    melb_lat, melb_long,
                    0, // auto zone
                    HEMISPHERE.None, // auto hemisphere
                                     out easting,
                                     out northing,
                                     out zoneStr, out zoneNum, out zoneLetter);
            Console.WriteLine($"AUTOMATIC:");
            Console.WriteLine($"easting:{easting:F2}, northing:{northing:F2}");
            Console.WriteLine($"zone: {zoneNum} letter:{zoneLetter}");



            // convert the other way
            UTM.UTM_to_LatLong(GLOBALDATUM.GDA94, zoneNum, zoneLetter, easting, northing, out var lat, out var lon, out var hem);
            Console.WriteLine($"UTM to LAT LONG:");
            Console.WriteLine($"lat:{lat:F2}, long:{lon:F2}");
            Console.WriteLine($"hemisphere: {hem}");
        }


        public static int GetBestZoneNumber(double[] latLong)
        {
            if (latLong == null) return 0;
            if (latLong.Length==0) return 0;
            if (latLong.Length%2 == 1) return 0;

            int[] zoneNumArray = new int[latLong.Length / 2];

            for (int i = 0; i < latLong.Length; i += 2)
            {
                var lat = latLong[i];
                var lon = latLong[i + 1];

                var zoneNum = UTM.GetZoneNumber(lat, lon);
                zoneNumArray[i / 2] = zoneNum;
            }


            var mostFreq = zoneNumArray.GroupBy(x => x)
                           .OrderByDescending(g => g.Count())
                           .First()
                           .Key;

            return mostFreq;
        }






        public static int GetZoneNumber(double lat, double @long)
        {
            // Make sure the longitude is between -180.00 .. 179.9
            var longTemp = _clampLogitudeTo180DegreeRange(@long);

            int zoneNumber = ((int)((longTemp + 180) / 6)) + 1;

            // mapping type fiddling (cultural, practical reasons rather than geometric ones)
            if (lat >= 56.0 && lat < 64.0 && longTemp >= 3.0 && longTemp < 12.0) zoneNumber = 32;

            // Special zones for Svalbard
            if (lat >= 72.0 && lat < 84.0)
            {
                if (longTemp >= 0.0 && longTemp < 9.0) zoneNumber = 31;
                else if (longTemp >= 9.0 && longTemp < 21.0) zoneNumber = 33;
                else if (longTemp >= 21.0 && longTemp < 33.0) zoneNumber = 35;
                else if (longTemp >= 33.0 && longTemp < 42.0) zoneNumber = 37;
            }

            return zoneNumber;
        }


        // 0,  '\0'

        public static void LatLong_to_UTM(GLOBALDATUM globaldatum,
            double lat, double @long,
            int fixedZoneNumber, // zero is automatic
            HEMISPHERE preferredHemisphere,
            out double utmEasting,
            out double utmNorthing,
            out string zoneStr, out int zoneNumber, out char zoneLetter)
        {
            var sys = _sysList.FirstOrDefault(s => s.Datum == globaldatum);
            if (sys == null)
            {   // failed!
                utmNorthing = utmEasting = Double.NaN; zoneStr = null; zoneNumber = 0;
                zoneLetter = '\0';
                return;
            }


            zoneNumber = GetZoneNumber(lat, @long);

            if (fixedZoneNumber >= 1 && fixedZoneNumber <= 60)
                zoneNumber = fixedZoneNumber;


            var longOriginRad = ((zoneNumber - 1) * 6 - 180 + 3) * deg2rad;


            // compute the UTM Zone from the latitude and longitude
            zoneLetter = UTMLetterDesignator(lat);
            zoneStr = $"{zoneNumber}{zoneLetter}";

            var eccSquared = sys.eccSquared;
            var eccPrimeSquared = (eccSquared) / (1 - eccSquared);

            const double k0 = 0.9996;
            var latRad = lat * deg2rad;
            var longTemp = _clampLogitudeTo180DegreeRange(@long);
            var longRad = longTemp * deg2rad;
            var n = sys.a / Math.Sqrt(1 - eccSquared * Math.Sin(latRad) * Math.Sin(latRad));
            var T = Math.Tan(latRad) * Math.Tan(latRad);
            var c = eccPrimeSquared * Math.Cos(latRad) * Math.Cos(latRad);
            var A = Math.Cos(latRad) * (longRad - longOriginRad);

            var m = sys.a * ((1 - eccSquared / 4 - 3 * eccSquared * eccSquared / 64 - 5 * eccSquared * eccSquared * eccSquared / 256) * latRad
                         - (3 * eccSquared / 8 + 3 * eccSquared * eccSquared / 32 + 45 * eccSquared * eccSquared * eccSquared / 1024) * Math.Sin(2 * latRad)
                         + (15 * eccSquared * eccSquared / 256 + 45 * eccSquared * eccSquared * eccSquared / 1024) * Math.Sin(4 * latRad)
                         - (35 * eccSquared * eccSquared * eccSquared / 3072) * Math.Sin(6 * latRad));

            utmEasting = k0 * n * (A + (1 - T + c) * A * A * A / 6
                                     + (5 - 18 * T + T * T + 72 * c - 58 * eccPrimeSquared) * A * A * A * A * A / 120)
                         + 500000.0;

            utmNorthing = k0 * (m + n * Math.Tan(latRad) * (A * A / 2 + (5 - T + 9 * c + 4 * c * c) * A * A * A * A / 24
                                                                      + (61 - 58 * T + T * T + 600 * c - 330 * eccPrimeSquared) * A * A * A * A * A * A / 720));
            if (preferredHemisphere == HEMISPHERE.Southern)
            {
                utmNorthing += 10000000.0;
            }
            if (preferredHemisphere == HEMISPHERE.None)
            {
                if (lat < 0)
                    utmNorthing += 10000000.0; //10000000 meter offset for southern hemisphere
            }
        }






        public static void UTM_to_LatLong(GLOBALDATUM globaldatum,
            int zoneNumber, char zoneLetter,
            double utmEasting,
            double utmNorthing,
            out double latitude, out double longitude,
            out HEMISPHERE hemisphere)
        {
            //converts UTM coords to lat/long.  Equations from USGS Bulletin 1532 
            //East Longitudes are positive, West longitudes are negative. 
            //North latitudes are positive, South latitudes are negative
            //Lat and Long are in decimal degrees. 
            //original Written by Chuck Gantz- chuck.gantz@globalstar.com
            //adapted to QI work by James Parsons
            var sys = _sysList.FirstOrDefault(s => s.Datum == globaldatum);
            if (sys == null)
            {
                latitude = longitude = double.NaN;
                hemisphere = HEMISPHERE.None;
                return;
            }
            const double k0 = 0.9996;
            var a = sys.a; // ellipsoid[ReferenceEllipsoid].EquatorialRadius;
            //double eccSquared = ellipsoid[ReferenceEllipsoid].eccentricitySquared;
            var eccSquared = sys.eccSquared;
            var e1 = (1 - Math.Sqrt(1 - eccSquared)) / (1 + Math.Sqrt(1 - eccSquared));

            var x = utmEasting - 500000.0;
            var y = utmNorthing;

            if (zoneLetter >= 'N') hemisphere = HEMISPHERE.Northern;
            else
            {
                hemisphere = HEMISPHERE.Southern;//point is in southern hemisphere
                y -= 10000000.0; //remove 10,000,000 meter offset used for southern hemisphere
            }

            double LongOrigin = (zoneNumber - 1) * 6 - 180 + 3;

            var eccPrimeSquared = (eccSquared) / (1 - eccSquared);

            var M = y / k0;
            var mu = M / (a * (1 - eccSquared / 4 - 3 * eccSquared * eccSquared / 64 - 5 * eccSquared * eccSquared * eccSquared / 256));

            var phi1Rad = mu + (3 * e1 / 2 - 27 * e1 * e1 * e1 / 32) * Math.Sin(2 * mu)
                             + (21 * e1 * e1 / 16 - 55 * e1 * e1 * e1 * e1 / 32) * Math.Sin(4 * mu)
                             + (151 * e1 * e1 * e1 / 96) * Math.Sin(6 * mu);
            //var phi1 = phi1Rad * rad2deg;

            var N1 = a / Math.Sqrt(1 - eccSquared * Math.Sin(phi1Rad) * Math.Sin(phi1Rad));
            var T1 = Math.Tan(phi1Rad) * Math.Tan(phi1Rad);
            var C1 = eccPrimeSquared * Math.Cos(phi1Rad) * Math.Cos(phi1Rad);
            var R1 = a * (1 - eccSquared) / Math.Pow(1 - eccSquared * Math.Sin(phi1Rad) * Math.Sin(phi1Rad), 1.5);
            var D = x / (N1 * k0);

            latitude = phi1Rad - (N1 * Math.Tan(phi1Rad) / R1) * (D * D / 2 - (5 + 3 * T1 + 10 * C1 - 4 * C1 * C1 - 9 * eccPrimeSquared) * D * D * D * D / 24
                            + (61 + 90 * T1 + 298 * C1 + 45 * T1 * T1 - 252 * eccPrimeSquared - 3 * C1 * C1) * D * D * D * D * D * D / 720);
            latitude = latitude * rad2deg;

            longitude = (D - (1 + 2 * T1 + C1) * D * D * D / 6 + (5 - 2 * C1 + 28 * T1 - 3 * C1 * C1 + 8 * eccPrimeSquared + 24 * T1 * T1)

                            * D * D * D * D * D / 120) / Math.Cos(phi1Rad);
            longitude = LongOrigin + longitude * rad2deg;

        }



        public static char[] ValidZoneLetters = new[]
        {
            'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P',
            'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X',

        };



        public static GridSquare[] GetAdjacentGridSquares(int zoneNum, char zoneLetter, int layer = 1)
        {
            var res = new List<GridSquare>();
            _getAdjacentGridSquares(res, zoneNum, zoneLetter, layer);
            var arr = res.DistinctBy(g => new { g.Number, g.Letter }).ToArray();
            return arr;
        }

        private static void _getAdjacentGridSquares(List<GridSquare> res, int zoneNum, char zoneLetter, int layer)
        {
            // some square don't exist, but that's ok in this case
            var idx = _getLetterIndex(zoneLetter);
            if (idx == -1) return;

            var tmp = new List<GridSquare>
            {
                new GridSquare(zoneNum, ValidZoneLetters[idx])
            };

            if (layer == 0)
            {
                if (!res.Contains(tmp[0])) res.Add(tmp[0]);
                return;
            }
            layer--;

            var iE = zoneNum - 1;
            if (iE <= 0) iE += 60;
            var iW = zoneNum + 1;
            if (iW > 60) iW -= 60;

            if (idx == 0)
            {
                tmp.Add(new GridSquare(iE, ValidZoneLetters[idx]));
                tmp.Add(new GridSquare(iW, ValidZoneLetters[idx]));
                tmp.Add(new GridSquare(iE, ValidZoneLetters[idx + 1]));
                tmp.Add(new GridSquare(zoneNum, ValidZoneLetters[idx + 1]));
                tmp.Add(new GridSquare(iW, ValidZoneLetters[idx + 1]));
            }
            else if (idx == ValidZoneLetters.Length - 1)
            {
                tmp.Add(new GridSquare(iE, ValidZoneLetters[idx - 1]));
                tmp.Add(new GridSquare(zoneNum, ValidZoneLetters[idx - 1]));
                tmp.Add(new GridSquare(iW, ValidZoneLetters[idx - 1]));
                tmp.Add(new GridSquare(iE, ValidZoneLetters[idx]));
                tmp.Add(new GridSquare(iW, ValidZoneLetters[idx]));
            }
            else
            {
                tmp.Add(new GridSquare(iE, ValidZoneLetters[idx - 1]));
                tmp.Add(new GridSquare(zoneNum, ValidZoneLetters[idx - 1]));
                tmp.Add(new GridSquare(iW, ValidZoneLetters[idx - 1]));
                tmp.Add(new GridSquare(iE, ValidZoneLetters[idx]));
                tmp.Add(new GridSquare(iW, ValidZoneLetters[idx]));
                tmp.Add(new GridSquare(iE, ValidZoneLetters[idx + 1]));
                tmp.Add(new GridSquare(zoneNum, ValidZoneLetters[idx + 1]));
                tmp.Add(new GridSquare(iW, ValidZoneLetters[idx + 1]));
            }

            foreach (var gS in tmp)
            {
                if (!res.Contains(gS)) res.Add(gS);
                else gS.Number = 0;
            }


            foreach (var gS in tmp)
            {
                if (gS.Number == 0) continue;
                _getAdjacentGridSquares(res, gS.Number, gS.Letter, layer);
            }


        }


        public static GridSquare[] GetAdjacentGridSquares(int zoneNum, char zoneLetter, int hW, int hH)
        {
            var res = new List<GridSquare>();

            var zone0 = zoneNum - hW + 1;
            var zone1 = zoneNum + hW - 1;

            var letterIdx = _getLetterIndex(zoneLetter);
            var letterIdx0 = letterIdx - hH + 1;
            var letterIdx1 = letterIdx + hH - 1;
            if (letterIdx0 < 0) letterIdx0 = 0;
            if (letterIdx1 >= ValidZoneLetters.Length) letterIdx1 = ValidZoneLetters.Length - 1;

            for (var i = letterIdx0; i <= letterIdx1; i++)
            {
                zoneLetter = ValidZoneLetters[i];
                foreach (var num in UTM.ValidZoneNumbers)
                {
                    if (IsInZoneRange(num, zone0, zone1))
                        res.Add(new GridSquare(num, zoneLetter));
                }
            }

            return res.ToArray();
        }

        public static bool IsInZoneRange(int zone, int zoneMin, int zoneMax)
        {
            if (zone >= zoneMin && zone <= zoneMax) return true;
            zone += 60;
            if (zone >= zoneMin && zone <= zoneMax) return true;
            zone -= 120;
            if (zone >= zoneMin && zone <= zoneMax) return true;
            return false;
        }


        private static char UTMLetterDesignator(double lat)
        {
            char letterDesignator;

            if ((84 >= lat) && (lat >= 72)) letterDesignator = 'X';
            else if ((72 > lat) && (lat >= 64)) letterDesignator = 'W';
            else if ((64 > lat) && (lat >= 56)) letterDesignator = 'V';
            else if ((56 > lat) && (lat >= 48)) letterDesignator = 'U';
            else if ((48 > lat) && (lat >= 40)) letterDesignator = 'T';
            else if ((40 > lat) && (lat >= 32)) letterDesignator = 'S';
            else if ((32 > lat) && (lat >= 24)) letterDesignator = 'R';
            else if ((24 > lat) && (lat >= 16)) letterDesignator = 'Q';
            else if ((16 > lat) && (lat >= 8)) letterDesignator = 'P';
            else if ((8 > lat) && (lat >= 0)) letterDesignator = 'N';
            else if ((0 > lat) && (lat >= -8)) letterDesignator = 'M';
            else if ((-8 > lat) && (lat >= -16)) letterDesignator = 'L';
            else if ((-16 > lat) && (lat >= -24)) letterDesignator = 'K';
            else if ((-24 > lat) && (lat >= -32)) letterDesignator = 'J';
            else if ((-32 > lat) && (lat >= -40)) letterDesignator = 'H';
            else if ((-40 > lat) && (lat >= -48)) letterDesignator = 'G';
            else if ((-48 > lat) && (lat >= -56)) letterDesignator = 'F';
            else if ((-56 > lat) && (lat >= -64)) letterDesignator = 'E';
            else if ((-64 > lat) && (lat >= -72)) letterDesignator = 'D';
            else if ((-72 > lat) && (lat >= -80)) letterDesignator = 'C';
            else letterDesignator = 'Z'; //Latitude is outside the UTM limits
            return letterDesignator;
        }

        private static double _clampLogitudeTo180DegreeRange(double @long)
        {
            return (@long + 180) - ((int)((@long + 180) / 360)) * 360 - 180;
        }


        private static int _getLetterIndex(char zoneLetter)
        {
            for (var i = 0; i < ValidZoneLetters.LongLength; i++)
            {
                if (ValidZoneLetters[i] == zoneLetter) return i;
            }

            return -1;
        }


        public static void GetLatLongRange(int zoneNumber, char letter, out double lat0, out double lat1, out double lon0, out double lon1)
        {
            if (letter == 'C') { lat0 = -80; lat1 = -72; }
            else if (letter == 'D') { lat0 = -72; lat1 = -64; }
            else if (letter == 'E') { lat0 = -64; lat1 = -56; }
            else if (letter == 'F') { lat0 = -56; lat1 = -48; }
            else if (letter == 'G') { lat0 = -48; lat1 = -40; }
            else if (letter == 'H') { lat0 = -40; lat1 = -32; }
            else if (letter == 'J') { lat0 = -32; lat1 = -24; }
            else if (letter == 'K') { lat0 = -24; lat1 = -16; }
            else if (letter == 'L') { lat0 = -16; lat1 = -8; }
            else if (letter == 'M') { lat0 = -8; lat1 = 0; }
            else if (letter == 'N') { lat0 = 0; lat1 = 8; }
            else if (letter == 'P') { lat0 = 8; lat1 = 16; }
            else if (letter == 'Q') { lat0 = 16; lat1 = 24; }
            else if (letter == 'R') { lat0 = 24; lat1 = 32; }
            else if (letter == 'S') { lat0 = 32; lat1 = 40; }
            else if (letter == 'T') { lat0 = 40; lat1 = 48; }
            else if (letter == 'U') { lat0 = 48; lat1 = 56; }
            else if (letter == 'V') { lat0 = 56; lat1 = 64; }
            else if (letter == 'W') { lat0 = 64; lat1 = 72; }
            else if (letter == 'X') { lat0 = 72; lat1 = 84; }
            else { lat0 = lat1 = Double.NaN; }

            // zone 1 : -180 to -174
            lon0 = (zoneNumber - 1) * 6 - 180;
            lon1 = lon0 + 6;

            // fiddly bits
            // norway sea
            if (letter == 'V')
            {
                if (zoneNumber == 31)
                {
                    lon0 = 0;
                    lon1 = 9;
                }
                else if (zoneNumber == 32)
                {
                    lon0 = 3;
                    lon1 = 12;
                }
            }

            // Special zones for Svalbard
            if (letter == 'X')
            {
                if (zoneNumber == 31)
                {
                    lon0 = 0;
                    lon1 = 9;
                }
                else if (zoneNumber == 33)
                {
                    lon0 = 9;
                    lon1 = 21;
                }
                else if (zoneNumber == 35)
                {
                    lon0 = 21;
                    lon1 = 33;
                }
                else if (zoneNumber == 37)
                {
                    lon0 = 33;
                    lon1 = 42;
                }
            }
        }
    }




    public enum GLOBALDATUM
    {
        NONE,
        WGS84,
        WGS66,
        WGS72,
        GRS80,
        AGD66,// 	6378160.000 298.25
        AGD84,// 	6378160.000 298.25
        GDA94, //6378137  	298.2572221
        GDA2000,//6378137  	298.2572221
    }

    public enum HEMISPHERE
    {
        Northern,
        Southern,
        Eastern,
        Western,
        None,

    }

    class CoordSysDefinition
    {
        public readonly GLOBALDATUM Datum;
        public readonly double a;
        public readonly double b;
        public readonly double eccSquared;
        public readonly double InvFlat;

        public CoordSysDefinition(GLOBALDATUM gdatum, double a, double invFlat)
        {
            this.Datum = gdatum;
            this.a = a;
            InvFlat = invFlat;
            b = a * (1.0 - 1.0 / invFlat);
            eccSquared = 1 - Math.Pow((invFlat - 1) / invFlat, 2);
        }
    }

    public class GridSquare
    {
        public GridSquare(int number, char letter)
        {
            Number = number;
            Letter = letter;
        }

        public int Number { get; set; }
        public char Letter { get; }

        public override bool Equals(object obj)
        {
            var gS = obj as GridSquare;
            if (gS == null) return false;
            return gS.Letter == Letter && gS.Number == Number;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;

                hash = hash * 23 + Letter.GetHashCode();
                hash = hash * 23 + Number.GetHashCode();
                return hash;
            }
        }
    }
}













