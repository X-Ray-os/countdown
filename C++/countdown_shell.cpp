#include <iostream>
#include <time.h>
#include <thread>
#include <chrono>
#include <future>
#include <atomic>
#include "countdown_shell.hpp"
using namespace std;

tuple<string, string> get_args(){
    cout << "输入倒计时名称：";
    string name;
    cin >> name;
    cout << "输入倒计时时长：";
    string seconds;
    cin >> seconds;
    while (true){
        try {
            int span = parse_span(seconds);
            if (span <= 0){
                throw invalid_argument("倒计时时长必须为正整数");
            }
        }catch (const invalid_argument& e){
            cout << "输入错误：" << e.what() << endl;
            cout << "请重新输入倒计时时长：";
            cin >> seconds;
            continue;
        }        break;
    }
    return make_tuple(name, seconds);
}

enum class State { RUNNING, PAUSED, STOPPED, RESTART };

class BrickPrinter{
    public:
    const int length = 30;
    const char symbol = '█';
    Countdown& cd;
    BrickPrinter(Countdown& c) : cd(c) {}
    void print_bricks(){
        int total = cd.getTotal();
        int remain = cd.getRemain();
        int current = ((total - remain) * length) / total;
        cout << "\r[";
        for(int i=0; i<current; i++) cout << symbol;
        for(int i=current; i<length; i++) cout << ' ';
        cout << "] " << remain << "s" << flush;
    }
};

void countdown(string name, int seconds){
    Countdown cd(seconds);
    BrickPrinter bp(cd);
    std::atomic<State> state = State::RUNNING;
    auto get_input = []() -> char {
        char cmd;
        std::cin >> cmd;
        return cmd;
    };
    std::future<char> input_future;
    bool waiting_for_input = false;
    bp.print_bricks(); // initial print
    while (state != State::STOPPED){
        if (!waiting_for_input){
            input_future = std::async(std::launch::async, get_input);
            waiting_for_input = true;
        }
        if (waiting_for_input && input_future.wait_for(std::chrono::seconds(0)) == std::future_status::ready){
            try {
                char cmd = input_future.get();
                waiting_for_input = false;
                if (cmd == 'p' || cmd == 'P') state = State::PAUSED;
                else if (cmd == 'c' || cmd == 'C') state = State::RUNNING;
                else if (cmd == 'r' || cmd == 'R') state = State::RESTART;
                else if (cmd == 's' || cmd == 'S') state = State::STOPPED;
            } catch (...) {}
        }
        if (state == State::RUNNING){
            std::this_thread::sleep_for(std::chrono::seconds(1));
            bp.print_bricks();
            if (cd.getRemain() <= 0){
                state = State::STOPPED;
            }
        } else if (state == State::PAUSED){
            std::this_thread::sleep_for(std::chrono::seconds(1));
        } else if (state == State::RESTART){
            cd.reset();
            state = State::RUNNING;
        }
    }
    cout << endl << name << "倒计时结束！" << endl;
}