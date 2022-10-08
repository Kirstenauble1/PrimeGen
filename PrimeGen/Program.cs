using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;

/**
* @author Kirsten Auble
* PrimeGen generates specified number of primes.
*/

/*
 * PrimeGen uses the Miller-Rabin primality test to return
 * primality of large integers.
 */
static class PrimeGen
{
    private static int byteLength;
    private static Object locker = new object();

    static Boolean IsProbablyPrime(this BigInteger value, int k = 10)
    {
        BigInteger two = new BigInteger(2);
        Random rnd = new Random();
        
        // find d
        BigInteger r = BigInteger.Zero;
        BigInteger valMinusOne = BigInteger.Subtract(value, BigInteger.One);
        BigInteger d = valMinusOne;
        while (BigInteger.Remainder(d, two) == BigInteger.Zero)
        {
            d = BigInteger.Divide(d, two);
            r = BigInteger.Add(r, BigInteger.One);
        }
        

        // witness loop
        for (int i = 0; i < k; i++)
        {
            // a is negative numbers 
            BigInteger a = two;
            while (a <= two)
            {
                Byte[] array = RandomNumberGenerator.GetBytes(rnd.Next(4, byteLength));
                a = new BigInteger(array);
            }
            BigInteger x = BigInteger.ModPow(a, d, value);

            if (x == BigInteger.One || x == valMinusOne)
            {
                continue;
            }

            bool contflag = false;

            for (int j = 0; j < r; j++)
            {
                x = BigInteger.ModPow(x, two, value);
                
                if (x == valMinusOne)
                {
                    contflag = true;
                    break;
                }
            }

            if (contflag)
            {
                continue;
            }
            return false;
        }
        return true;

    }

    /*
     * if sum of digits % 3 == 0, cannot be prime
     */
    static Boolean sumDigits(this BigInteger value)
    {
        int sumOfDigits = 0;
        String digits = value.ToString();
        for (int i = 0; i < digits.Length; i++)
        {
            sumOfDigits += digits[i] - '0';

        }

        BigInteger remainder;
        BigInteger.DivRem(value, new BigInteger(3), out remainder);

        if (remainder == BigInteger.Zero)
        {
            return false;
        }

        return true;
    }
    
    /*
     * Prints standard error message
     */
    static void PrintError()
    {
        Console.WriteLine("Incorrect arguments provided. For help run dotnet run -h.\n");
        Environment.Exit(0);
    }

    /*
     * Prints standard help message
     */
    static void PrintHelp()
    {
        Console.WriteLine("dotnet run <bits> <count=1>\n");
        Console.WriteLine("\t- bits - the number of bits of the prime number, this must be a");
        Console.WriteLine("\tmultiple of 8, and at least 32 bits.");
        Console.WriteLine("\t- count - the number of prime numbers to generate, defaults to 1");
        Environment.Exit(0);
    }
    
    static void Main(string[] args)
    {
        
        // if user inputs help command
        if (args.Length == 1 && args[0] == "-h")
        {
            PrintHelp();
        }
        
        // check to verify that all args are numeric
        foreach (var arg in args)
        {
            int check;
            if (!int.TryParse(arg, out check))
            {
                PrintError();
            }
        }
            
        // count may or not be given
        if (args.Length is 2 or 1)
        {
            int count = 1;
            int bits = Int32.Parse(args[0]);
            
            // count provided
            if (args.Length == 2)
            {
                count = Int32.Parse(args[1]);
            }

            if (bits % 8 != 0 || bits < 32)
            {
                PrintError();
            }

            int bytes = bits / 8;

            byteLength = bytes;

            int idx = 1;
            
            Console.WriteLine("BitLength: " + args[0] + " bits");

            var options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = 5
            };

            BigInteger two = new BigInteger(2);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            Parallel.For(0, count, options, i =>
            {
                while (true)
                {
                    Byte[] array = RandomNumberGenerator.GetBytes(bytes);
                    var bi = new BigInteger(array);
                    BigInteger remainder = BigInteger.Remainder(bi, two);
                    if (remainder != BigInteger.Zero && bi > BigInteger.Zero)
                    {
                        if (sumDigits(bi))
                        {
                            if (bi.IsProbablyPrime())
                            {
                                lock (locker)
                                {
                                    Console.Write(idx + ": ");
                                    Console.WriteLine(bi);
                                    if (idx != count)
                                    {
                                        Console.Write("\n");
                                    }
                                }

                                Interlocked.Increment(ref idx);
                                break;
                            }
                        }
                    }
                }
            });

          
            watch.Stop();
            TimeSpan ts = watch.Elapsed;
            Console.WriteLine("Time to Generate: " + ts.ToString("hh\\:mm\\:ss\\.fffffff"));

        }
        else
        {
            PrintError();
        }

    }
}