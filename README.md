# UTM
UTM / Latitude Longitude Conversion


see:
http://cholla.mmto.org/gtopo/reference/utm/convert.html
https://www.dmap.co.uk/utmworld.htm

check your values with:
http://rcn.montana.edu/resources/Converter.aspx

GDA94 and WGS84 are very similar:
Read:"What is the difference between GDA94 and WGS84?" on 
http://www.geoproject.com.au/gda.faq.html#q09




static public void Test()
  just a test 


ZONE QUERY METHODS ---------------------------------------------------------

 public static int GetZoneNumber(double lat, double @long)
     get the zone number from the logitude (with mods for Svalbard/Norway area)
 
 
 public static int GetBestZoneNumber(double[] latLong)
     similar to GetZoneNumber(), but takes the most frequent zone from a list of lat/longs
 
 
 
 MAIN CONVERSION METHODS --------------------------------------------------
 
 public static void LatLong_to_UTM(GLOBALDATUM globaldatum,
         double lat, double @long,
         int fixedZoneNumber, // zero is automatic
         HEMISPHERE preferredHemisphere,
         out double utmEasting,
         out double utmNorthing,
         out string zoneStr, out int zoneNumber, out char zoneLetter)
     from lat/long and projection and zone info, get the easting, northing and zone values



public static void UTM_to_LatLong(GLOBALDATUM globaldatum,
         int zoneNumber, char zoneLetter,
         double utmEasting,
         double utmNorthing,
         out double latitude, out double longitude,
         out HEMISPHERE hemisphere)
     from easting/northing etc, get the lat/long



 UTILITY METHODS ----------------------------------------------------------

public static char[] ValidZoneLetters
     a list of the valid zone charaters


public static bool IsInZoneRange(int zone, int zoneMin, int zoneMax)
     determine if zone is in a zone range. Takes account of wrap around through the Pacific.
     good if you live in Fiji
     
     
public static char UTMLetterDesignator(double lat)
     Letters designated to latitude bands (eg England in U, Mexico in Q, Antartica in D and C)

 
public static GridSquare[] GetAdjacentGridSquares(int zoneNum, char zoneLetter, int layer = 1)
     Get the adjacent grid squares to supplied zoneNum/zoneLetter 
     layers is how many layers out to search. 
     
public static GridSquare[] GetAdjacentGridSquares(int zoneNum, char zoneLetter, int hW, int hH)
     Get the adjacent grid squares to supplied zoneNum/zoneLetter 
     hW and hH determines amounts above and below
     
public static void GetLatLongRange(int zoneNumber, char letter, out double lat0, out double lat1, out double lon0, out double lon1)
     Get the lat/long limits of a particular square

