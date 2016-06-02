use strict;

=for comment 
timehash.pl - A library by HumanGeo to help compute variable precision time intervals,
for use in Big Data analysis, spatial-temporal computation, and other quantitative data analysis.
Copyright (C) 2014  HumanGeo abe@thehumangeo.com

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
    * Redistributions of source code must retain the above copyright
      notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright
      notice, this list of conditions and the following disclaimer in the
      documentation and/or other materials provided with the distribution.
    * Neither the name of HumanGeo nor the
      names of its contributors may be used to endorse or promote products
      derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL HUMANGEO BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
=cut

my @base32 = qw(0 1 a b c d e f);
my %decodemap = {};
for (my $i = 0; $i < scalar(@base32); $i++)
{
    $decodemap{$base32[$i]} = $i;
}

#0 +/- 64 years
#1 +/- 8 years
#2 +/- 1 years
#3 +/- 45.65625 days
#4 +/- 5.707 days
#5 +/- 0.71337 days = 17.121 hours
#6 +/- 2.14013671875 hours
#7 +/- 0.26751708984375 hours = 16.05 minutes
#8 +/- 2.006378173828125 minutes
#9 +/- 0.2507 minutes = 15 seconds
#10 +/- 1.88097 seconds"""

sub decode_exactly
{
    #Decode the timehash to its exact values, including the error
    #margins of the result.  Returns two float values: timehash
    #and the plus/minus error for epoch seconds.
    my ($timehash) = @_;
    my @time_interval = (0.0, 4039372800.0);
    my $time_error = ($time_interval[0] + $time_interval[1])/2;  #from January 1, 1970 to January 1, 2098
    for my $c (split(//, $timehash))
    {
        my $cd = $decodemap{$c};
        for my $mask ((4, 2, 1))
        {
            $time_error /= 2;
            my $mid = ($time_interval[0] + $time_interval[1])/2;
            if ($cd & $mask)
            {
                @time_interval = ($mid, $time_interval[1]);
            }
            else
            {
                @time_interval = ($time_interval[0], $mid);
            }
        }
    }
    my $time_value = ($time_interval[0] + $time_interval[1])/2;
    return ($time_value, $time_error);
}

sub decode
{
    #Decode timehash, returning a single floating point value for epoch seconds.
    my ($epoch_seconds, $time_error) = decode_exactly(@_);
    #drop the time_error for now
    return $epoch_seconds;
}

sub encode
{
    #Encode a timestamp given as a floating point epoch time to
    #a timehash which will have the character count precision.
    my ($timeseconds, $precision) = @_;
    $precision = 10 if (!$precision);
    
    my @time_interval = (0.0, 4039372800.0); #from January 1, 1970 to January 1, 2098
    my @timehash = ();
    my @bits = (4, 2, 1);
    my $bit = 0;
    my $ch = 0;        
    while (scalar(@timehash) < $precision)
    {                
        my $mid = ($time_interval[0] + $time_interval[1])/2;       
        if ($timeseconds > $mid)
        {
            $ch |= $bits[$bit];            
            @time_interval = ($mid, $time_interval[1]);
        }
        else
        {
            @time_interval = ($time_interval[0], $mid);                
        }
        if ($bit < 2)
        {
            $bit += 1;
        }
        else
        {
            push(@timehash,$base32[$ch]);
            $bit = 0;
            $ch = 0;
        }
    }
    return join("", @timehash);
}

sub main
{
    #Main function - entry point into script.  Used for examples and testing.

    # Examples of encoding
    my $rightnow = time;
    my $rightnow_hash = &encode($rightnow);
    
    my $rightnow60 = $rightnow + 60;
    my $rightnow60_hash = &encode($rightnow60);    
    
    my $rightnow3600 = $rightnow + 3600;
    my $rightnow3600_hash = &encode($rightnow3600);
    
    my $previous3600 = $rightnow - 3600;
    my $previous3600_hash = &encode($previous3600);
    
    my $previous60 = $rightnow - 60;
    my $previous60_hash = &encode($previous60);
    
    my $previous86400 = $rightnow - 86400;
    my $previous86400_hash = &encode($previous86400);
    
    my $year_future_from_now = $rightnow + (86400*365.25);
    my $year_future_from_now_hash = &encode($year_future_from_now);
    
    print "one day ago\t\t$previous86400_hash\n";
    print "one hour ago\t\t$previous3600_hash\n";
    print "60 seconds ago\t\t$previous60_hash\n";
    print "now\t\t\t$rightnow_hash\n";
    print "60 seconds future\t$rightnow60_hash\n";
    print "one hour in the future\t$rightnow3600_hash\n";
    print "one year from today\t$year_future_from_now_hash\n";
    print "\n";
    
    # Example of decoding and error check
    my ($rightnow_calculated, $time_error) = &decode_exactly($rightnow_hash);
    print "original rightnow = $rightnow\n";
    print "calculated rightnow = $rightnow_calculated\n";
    print "time error = $time_error\n";
}

&main();
