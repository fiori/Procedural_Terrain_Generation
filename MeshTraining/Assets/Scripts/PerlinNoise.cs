using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PerlinNoise
{
    public static float OctavePerlin(float x, float y, float z, float scale, int octaves, float persistance, float lacunarity)
    {
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0;
        for (int i = 0; i < octaves; i++)
        {
            float valueX = x / scale * frequency;
            float valueY = y / scale * frequency;
            float valueZ = z / scale * frequency;
            total += Perlin(valueX, valueY, valueZ) * amplitude;

            maxValue += amplitude;

            amplitude *= persistance;
            frequency *= lacunarity;
        }

        return total / maxValue;
    }


    // Hash lookup table as defined by Ken Perlin.  This is a randomly
    // arranged array of all numbers from 0-255 inclusive.
    public static readonly int[] Permutation = {
        151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69,
        142, 8, 99, 37, 240, 21, 10, 23,
        190, 6, 148, 247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177, 33,
        88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165, 71, 134, 139, 48, 27, 166,
        77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244,
        102, 143, 54, 65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 200, 196,
        135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64, 52, 217, 226, 250, 124, 123,
        5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42,
        223, 183, 170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9,
        129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104, 218, 246, 97, 228,
        251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107,
        49, 192, 214, 31, 181, 199, 106, 157, 184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254,
        138, 236, 205, 93, 222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180
    };

    private static readonly int[] p;

    static PerlinNoise()
    {
        p = new int[512]; // Used in a hash function
        for (int x = 0; x < 512; x++)
        {
            p[x] = Permutation[x % 256];
        }
    }

    public static float Perlin(float x, float y, float z)
    {
        //Start using Mathf.Floor() 
        //Mathf.Floor return the smallest integer number from the variable. For example if X = 10.7 Mathf.Floor(x) = 10
        //This resolved the problem, before the output was -30 sometimes and +24. With this function the problem was solved.
        //Source https://docs.unity3d.com/ScriptReference/Mathf.Floor.html
        int xi = (int)Mathf.Floor(x) & 255;
        int yi = (int)Mathf.Floor(y) & 255;
        int zi = (int)Mathf.Floor(z) & 255;
        x -= (int)Mathf.Floor(x);
        y -= (int)Mathf.Floor(y);
        z -= (int)Mathf.Floor(z);

        float u = fade(x);
        float v = fade(y);
        float w = fade(z);

        //Left Corner
        int A = p[xi] + yi;
        //Right Corner
        int B = p[xi + 1] + yi;
        
        //The 8 cube corners
        int AA = p[A] + zi;
        int AB = p[A+1] + zi;
        int BB = p[B+1] + zi;
        int BA = p[B] + zi;


        float x1, x2, y1, y2;
        //Check the front 4 corners of the cube
        x1 = lerp(grad(p[AA], x, y, z), grad(p[BA], x - 1, y, z), u);
        x2 = lerp(grad(p[AB], x, y - 1, z), grad(p[BB], x - 1, y - 1, z), u);
        y1 = lerp(x1, x2, v);
        //Check the back 4 corners of the cube, that's why is z-1.
        x1 = lerp(grad(p[AA+1], x, y, z-1), grad(p[BA+1], x - 1, y, z-1), u);
        x2 = lerp(grad(p[AB+1], x, y - 1, z-1), grad(p[BB+1], x - 1, y - 1, z-1), u);
        y2 = lerp(x1, x2, v);

        //+1/2 is added so the noise is never negative.
        return (lerp(y1, y2, w) + 1) /2;

        //int aa, ab, ba, bb;
        //aaa = p[p[p[xi] + yi] + zi];
        //aba = p[p[p[xi] + inc(yi)] + zi];
        //aab = p[p[p[xi] + yi] + inc(zi)];
        //abb = p[p[p[xi] + inc(yi)] + inc(zi)];
        //baa = p[p[p[inc(xi)] + yi] + zi];
        //bba = p[p[p[inc(xi)] + inc(yi)] + zi];
        //bab = p[p[p[inc(xi)] + yi] + inc(zi)];
        //bbb = p[p[p[inc(xi)] + inc(yi)] + inc(zi)];
        //aa = p[p[xi] + yi];
        //ab = p[p[xi] + inc(yi)];

        //ba = p[p[inc(xi)] + yi];
        //bb = p[p[inc(xi)] + inc(yi)];

        //aa = p[p[xi] + yi];
        //ab = p[p[xi] + yi + 1];

        //ba = p[p[xi + 1] + yi];
        //bb = p[p[xi + 1] + yi + 1];

        //float x1, x2, y1, y2;

        //x1 = lerp(grad(aa, xf, yf), grad(ba, xf - 1, yf), u);
        //x2 = lerp(grad(ab, xf, yf - 1), grad(bb, xf - 1, yf - 1), u);

        //y1 = lerp(x1, x2, v);

        //x1 = lerp(grad(aa, xf, yf), grad(bb, xf - 1, yf - 1), u);
        //x2 = lerp(grad(ab, xf, yf - 1), grad(ba, xf - 1, yf), u);

        //y2 = lerp(x1, x2, v);
    }

    //The Ken Perlin's original grade() function using complicated and confusing bit-flipping code to calculate the dot product of a randomly selected
    //gradient vector and the 8 location vectors.
    //Source:  https://mrl.nyu.edu/~perlin/noise/
    //public static float gradientKenPerlinOriginal(int hash, float x, float y, float z)
    //{
    //    int h = hash & 15; // Take the hased value and take the first 4 bits of it ( 15 == 0b1111)
    //    float u = h < 8 /* 0b1000 */ ? x : y; // If the most significant bit (MSB) of the hash is 0 then set u = x. Otherwise y.

    //    float v;

    //    if (h < 4 /* 0b0100 */) //If the first and second significant bits are 1 set v = y;
    //        v = y;
    //    else if (h == 12 /* 0b1100 */ || h == 14 /* 0b1110*/) //If the first and second significant bits are 1 set v = x
    //        v = x;
    //    else         //If the first and second significant btis are not equal (0/1, 1/0) set v = z
    //        v = z;

    //    return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v); //Use the last 2 bits to decide if u and v are positive or negative. Then return their addition

    //}

    //Easy and alternative way to write the gradient function
    // Source: http://riven8192.blogspot.com/2010/08/calculate-perlinnoise-twice-as-fast.html
    public static float grad(int hash, float x, float y, float z)
    {
        switch (hash & 0xF)
        {
            case 0x0: return x + y;
            case 0x1: return -x + y;
            case 0x2: return x - y;
            case 0x3: return -x - y;
            case 0x4: return x + z;
            case 0x5: return -x + z;
            case 0x6: return x - z;
            case 0x7: return -x - z;
            case 0x8: return y + z;
            case 0x9: return -y + z;
            case 0xA: return y - z;
            case 0xB: return -y - z;
            case 0xC: return y + x;
            case 0xD: return -y + z;
            case 0xE: return y - x;
            case 0xF: return -y - z;
            default: return 0; // never happens    
        }

        //Picks a random vector from the following 12 vectors:
        //(1, 1, 0),(-1, 1, 0),(1, -1, 0),(-1, -1, 0),
        //(1, 0, 1),(-1, 0, 1),(1, 0, -1),(-1, 0, -1),
        //(0, 1, 1),(0, -1, 1),(0, 1, -1),(0, -1, -1)
    }

    ////Easy and alternative way to write the gradient function
    //// Source: http://riven8192.blogspot.com/2010/08/calculate-perlinnoise-twice-as-fast.html
    //public static float grad(int hash, float x, float y)
    //{
    //    switch (hash & 0xF)
    //    {
    //        case 0x0: return x + y;
    //        case 0x1: return -x + y;
    //        case 0x2: return x - y;
    //        case 0x3: return -x - y;
    //        case 0x4: return y + x;
    //        case 0x5: return y - x;
    //        default: return 0; // never happens    
    //    }

    //    //Picks a random vector from the following 4 vectors:
    //    //(0,0),(0,1),(1,1)(1,0)
    //}

    //public static int inc(int num)
    //{
    //    num++;
    //    return num;
    //}

    //Linear Interpolation
    public static float lerp(float firstDouble, float secondDouble, float by)
    {
        return firstDouble + by * (secondDouble - firstDouble);
        //return firstDouble + (secondDouble - firstDouble) * by;
    }

    public static float fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10); // 6T^5 - 15T^4 + 10T^3
    }
}