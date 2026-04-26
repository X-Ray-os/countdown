#include <stdio.h>
#include <time.h>
#include <regex.h>
#include <time.h>
#include <windows.h>

int parse_span(string raw){
    regex_t re;
    regmatch_t matches[4];
    regcomp(&re, "([0-9]+)([smhd])", REG_EXTENDED);
    if (regexec(&re, raw.c_str(), 4, matches, 0) == 0) {
        int value = atoi(raw.substr(matches[1].rm_so, matches[1].rm_eo - matches[1].rm_so).c_str());
        char unit = raw[matches[2].rm_so];
        switch (unit) {
            case 's': return value;
            case 'm': return value * 60;
            case 'h': return value * 3600;
            case 'd': return value * 86400;
        }
    }
    return -1; // Invalid format
}

class Countdown {
    static void Countdown(){

    }
    public string name;
    public int duration;
    public bool status;
    protected double lastStartTime = 0;
    protected double remainingTime = 0;
    public Countdown(string name, string duration){
        this->name = name;
        this->duration = parse_span(duration);
        this->status = false;
    }
    public void start(){
        if (!status) {
            status = true;
            lastStartTime = clock() / (double)CLOCKS_PER_SEC;
        }
    }
    public void pause(){
        if (status) {
            status = false;
            double lastPauseTime = clock() / (double)CLOCKS_PER_SEC;
            remainingTime += lastPauseTime - lastStartTime;
        }
    }
    public double getRemain(){
        if (status) {
            double lastPauseTime = clock() / (double)CLOCKS_PER_SEC;
            return duration - (remainingTime + (lastPauseTime - lastStartTime));
        } else {
            return duration - remainingTime;
        }
    }
    public void reset(){
        status = false;
        lastStartTime = 0;
        remainingTime = duration;
    }
    double getProgress(){
        double elapsed = duration - getRemain();
        return (elapsed / duration);
    }
};