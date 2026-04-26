import time,tkinter,threading
from tkinter import ttk
import tkinter.messagebox as ms
# import sv_ttk

from countdown_base import parse_span, CountDown, cd_holder

def get_args():
    garoot = tkinter.Tk()
    nameVal = tkinter.StringVar()
    spanVal = tkinter.StringVar()
    tkinter.Label(garoot, text='Name:').grid(row=0, column=0)
    tkinter.Entry(garoot, textvariable=nameVal).grid(row=0, column=
                                                     1)
    tkinter.Label(garoot, text='Span:').grid(row=1, column=0)
    tkinter.Entry(garoot, textvariable=spanVal).grid(row=1, column=1)
    def submit():
        try:
            global span
            span = parse_span(spanVal.get())
        except Exception as e:
            ms.showerror('Error', f'Invalid span: {e}')
            return
        garoot.destroy()
    tkinter.Button(garoot, text='Submit', command=submit).grid(row=2, column=0, columnspan=2)
    garoot.mainloop()
    return nameVal.get(), span

def countdown_thread(name, span):
    cd = CountDown(name, span)
    cd_holder.append(cd)
    while cd.remaining > 0:
        time.sleep(1)
    ms.showinfo('Countdown Finished', f'Countdown "{name}" has finished!')

def countdown(name, span):
    (hold:=threading.Thread(target=countdown_thread, args=(name, span))).start()
    cd = CountDown(name, span)
    root = tkinter.Tk()
    tkinter.Label(root, text=f'Countdown: {name}').pack()
    remainVal = tkinter.StringVar()
    def update():
        remainVal.set(f'Remaining: {cd.remaining} seconds')
        if cd.remaining > 0:
            root.after(1000, update)

    update()
    tkinter.Label(root, textvariable=remainVal).pack()
    ttk.Progressbar(root, maximum=span, value=span-cd.remaining).pack()
    tkinter.Button(root, text='开始',command=cd.start).pack()
    tkinter.Button(root, text='暂停',command=cd.pause).pack()
    tkinter.Button(root, text='重置',command=cd.reset).pack()
    root.mainloop()

def main():
    name, span = get_args()
    countdown(name, span)

if __name__ == '__main__':
    main()