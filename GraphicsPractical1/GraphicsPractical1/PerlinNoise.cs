﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GraphicsPractical1
{
    // Perlin noise is a type of noise that looks cloudy.
    // The noise is generated by blending multiple layers of smooth noise (called 'octaves').
    // Few octaves will result in a 'jagged' appearance, while more octaves will result in a smoother appearance.
    // Source: http://devmag.org.za/2009/04/25/perlin-noise/
    class PerlinNoise
    {
        // Generate an array filled with random values between 0 and 1.
        public static float[][] GenerateWhiteNoise(int width, int height)
        {
            Random random = new Random();
            float[][] noise = GetEmptyArray<float>(width, height);
 
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    noise[i][j] = (float)random.NextDouble() % 1;
                }
            }
 
            return noise;
        }

        // Initialize an empty array.
        public static T[][] GetEmptyArray<T>(int width, int height)
        {
            T[][] image = new T[width][];

            for (int i = 0; i < width; i++)
            {
                image[i] = new T[height];
            }

            return image;
        }


        // Generate noise based on the base noise for a given octave.
        // For the k-th octave, the base noise is sampled at every point (i*2^k, j*2^k),
        // the other points are interpolated linearly.
        public static float[][] GenerateSmoothNoise(float[][] baseNoise, int octave)
        {
           int width = baseNoise.Length;
           int height = baseNoise[0].Length;
 
           float[][] smoothNoise = GetEmptyArray<float>(width, height);

           int samplePeriod = (int)Math.Pow(2, octave);
           float sampleFrequency = 1.0f / samplePeriod;
 
           for (int i = 0; i < width; i++)
           {
              // Calculate the horizontal sampling indices (i*2^k).
              int i0 = (i / samplePeriod) * samplePeriod;
              int i1 = (i0 + samplePeriod) % width; // Wrapping around horizontally.
              float horizontalBlend = (i - i0) * sampleFrequency;
 
              for (int j = 0; j < height; j++)
              {
                 // Calculate the vertical sampling indices (j*2^k).
                 int j0 = (j / samplePeriod) * samplePeriod;
                 int j1 = (j0 + samplePeriod) % height; // Wrapping around vertically. 
                 float verticalBlend = (j - j0) * sampleFrequency;
 
                 // Blend the two top corners.
                 float top = Interpolate(baseNoise[i0][j0],
                    baseNoise[i1][j0], horizontalBlend);
 
                 // Blend the two bottom corners.
                 float bottom = Interpolate(baseNoise[i0][j1],
                    baseNoise[i1][j1], horizontalBlend);
 
                 // Final blending.
                 smoothNoise[i][j] = Interpolate(top, bottom, verticalBlend);
              }
           }
 
           return smoothNoise;
        }

        // Calculate the in-between value of two other values.
        public static float Interpolate(float x0, float x1, float alpha)
        {
            return x0 * (1 - alpha) + alpha * x1;
        }


        // Generate the Perlin noise by blending the layers of smooth noise (octaves).
        // Every octave has a weight (amplitude), each successive octave will have an amplitude of:
        // previous amplitude / k for some value k. This value k is called the persistance.
        // After all the weighted values are added, normalize so that the noise will range from 0 to 1.
        public static float[][] GeneratePerlinNoise(float[][] baseNoise, int octaveCount)
        {
            int width = baseNoise.Length;
            int height = baseNoise[0].Length;

            float[][][] smoothNoise = new float[octaveCount][][];

            // Persistance is amount by which the weight of the octaves will be reduced.
            // A higher persistance will emphasize the first octaves, and thus result in a more 'jagged' scene.
            // A lower persistance will spread out the weights between more octaves, and thus result in a smoother scene.
            // A persistance around 0.6 is recommended.
            float persistance = 0.6f;

            // Generate the octaves.
            for (int i = 0; i < octaveCount; i++)
            {
                smoothNoise[i] = GenerateSmoothNoise(baseNoise, i);
            }

            float[][] perlinNoise = GetEmptyArray<float>(width, height);

            // The amplitudes of the octaves start at 1.
            float amplitude = 1.0f;
            float totalAmplitude = 0.0f;

            // Blend the octaves.
            for (int octave = octaveCount - 1; octave >= 0; octave--)
            {
                amplitude *= persistance;
                totalAmplitude += amplitude;

                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        perlinNoise[i][j] += smoothNoise[octave][i][j] * amplitude;
                    }
                }
            }

            // Normalization of the noise.
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    perlinNoise[i][j] /= totalAmplitude;
                }
            }

            return perlinNoise;
        }

        public static float[][] GeneratePerlinNoise(int width, int height, int octaveCount)
        {
            float[][] baseNoise = GenerateWhiteNoise(width, height);

            return GeneratePerlinNoise(baseNoise, octaveCount);
        }


        // Convert the range from 0-1 float to 0-255 byte greyscale values.
        public static byte[][] MapToGray(float[][] grayValues)
        {
            int width = grayValues.Length;
            int height = grayValues[0].Length;

            byte[][] image = GetEmptyArray<byte>(width, height);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    byte gray = (byte) (256 * grayValues[i][j]);
                    image[i][j] = gray;
                }
            }

            return image;
        }
    }
}
