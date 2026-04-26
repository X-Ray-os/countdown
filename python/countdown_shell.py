import time
from countdown_base import parse_span, CountDown, cd_holder

def get_args():
    name = input("请输入倒计时名称")
    raw = input("请输入倒计时时长")

    while True:
        try:
            span = parse_span(raw)
        except:
            raw = input("注意，格式应为xxhxxmxxs")
        else:
            break
    return name,span

def gen_brick_printer(cd:CountDown):
    LENGTH = 30
    SYMB = "■"
    printed = 0
    while True:
        increase = int( cd.percentage() * LENGTH )-printed
        print(SYMB*increase,end="")
        printed += increase
        yield

def countdown(name,span):
    cd = CountDown(name,span)
    hold = cd_holder(cd)
    cd.start()
    while True:

        bp = gen_brick_printer(cd)
        print("|-----|-----|-----|-----|-----|")
        try:
            while True:
                next(bp)
                next(hold)
                time.sleep(1)
        except KeyboardInterrupt:
            cd.pause()
            inq = input("已暂停")
            if inq == "k":
                # 终止
                break
            elif inq:
                # 重新开始
                cd.reset()
                input("按Enter以开始")
                cd.start()
            else:
                # 继续
                cd.start()

def main():
    name,span = get_args()
    print(name)
    countdown(name,span)

if __name__ == '__main__':
    main()