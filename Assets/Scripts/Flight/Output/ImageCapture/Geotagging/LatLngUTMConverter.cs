using System;

public class LatLngUTMConverter {

    public class LatLng {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    public static LatLng ConvertUtmToLatLng(double UTMEasting, double UTMNorthing, int UTMZoneNumber, string UTMZoneLetter) {
        // These are set according to WGS84
        double a = 6378137;
        double eccSquared = 0.00669438;

        var e1 = (1 - Math.Sqrt(1 - eccSquared)) / (1 + Math.Sqrt(1 - eccSquared));
        var x = UTMEasting - 500000.0; //remove 500,000 meter offset for longitude
        var y = UTMNorthing;
        var ZoneNumber = UTMZoneNumber;
        var ZoneLetter = UTMZoneLetter;

        if ("N" != ZoneLetter) {
            y -= 10000000.0;
        }

        var LongOrigin = (ZoneNumber - 1) * 6 - 180 + 3;

        var eccPrimeSquared = (eccSquared) / (1 - eccSquared);

        double M = y / 0.9996;
        var mu = M / (a * (1 - eccSquared / 4 - 3 * eccSquared * eccSquared / 64 - 5 * eccSquared * eccSquared * eccSquared / 256));

        var phi1Rad = mu + (3 * e1 / 2 - 27 * e1 * e1 * e1 / 32) * Math.Sin(2 * mu)
                + (21 * e1 * e1 / 16 - 55 * e1 * e1 * e1 * e1 / 32) * Math.Sin(4 * mu)
                + (151 * e1 * e1 * e1 / 96) * Math.Sin(6 * mu);
        var phi1 = ToDegrees(phi1Rad);

        var N1 = a / Math.Sqrt(1 - eccSquared * Math.Sin(phi1Rad) * Math.Sin(phi1Rad));
        var T1 = Math.Tan(phi1Rad) * Math.Tan(phi1Rad);
        var C1 = eccPrimeSquared * Math.Cos(phi1Rad) * Math.Cos(phi1Rad);
        var R1 = a * (1 - eccSquared) / Math.Pow(1 - eccSquared * Math.Sin(phi1Rad) * Math.Sin(phi1Rad), 1.5);
        var D = x / (N1 * 0.9996);

        var Lat = phi1Rad - (N1 * Math.Tan(phi1Rad) / R1) * (D * D / 2 - (5 + 3 * T1 + 10 * C1 - 4 * C1 * C1 - 9 * eccPrimeSquared) * D * D * D * D / 24
                + (61 + 90 * T1 + 298 * C1 + 45 * T1 * T1 - 252 * eccPrimeSquared - 3 * C1 * C1) * D * D * D * D * D * D / 720);
        Lat = ToDegrees(Lat);

        var Long = (D - (1 + 2 * T1 + C1) * D * D * D / 6 + (5 - 2 * C1 + 28 * T1 - 3 * C1 * C1 + 8 * eccPrimeSquared + 24 * T1 * T1)
                * D * D * D * D * D / 120) / Math.Cos(phi1Rad);
        Long = LongOrigin + ToDegrees(Long);
        return new LatLng { Lat = Lat, Lng = Long };
    }

    private static double ToDegrees(double rad) {
        return rad / Math.PI * 180;
    }
}
