import serial
import numpy as np
import time
from matplotlib import pyplot as plt
from matplotlib import animation

fig=plt.figure()
ax=plt.axes(xlim=(0,1000),ylim=(-2,2))
plt.xlabel('time')
plt.ylabel('acceleration')
particles1, =ax.plot([],[],'o',ms=6)
particles2, =ax.plot([],[],'o',ms=6)
particles3, =ax.plot([],[],'o',ms=6)

ser=serial.Serial(6,57600)
start=time.clock()

sp_x=0
sp_y=0
sp_z=0
disp_x=0
disp_y=0
disp_z=0

list_x=[]
list_y=[]
list_z=[]

ti=0
i=0
x1=[0, 0, 0, 0]
y1=[0, 0, 0, 0]
z1=[0, 0, 0, 0]



def getdata():
    global x1
    global y1
    global z1
    global sp_x
    global sp_y
    global sp_z
    global disp_x
    global disp_y
    global disp_z
    global i
    
    data=ser.readline()
    
    print(data)
    l=len(data)
    dat=data[0:l-2]
    d_st=bytes.decode(dat)
    d_sp=d_st.split(",")
    x=d_sp[0].split("=")
    y=d_sp[1].split("=")
    z=d_sp[2].split("=")

    x1[0]=x1[1]
    y1[0]=y1[1]
    z1[0]=z1[1]
    x1[1]=x1[2]
    y1[1]=y1[2]
    z1[1]=z1[2]
    x1[2]=x1[3]
    y1[2]=y1[3]
    z1[2]=z1[3]
    
    x1[3]=(float(x[1])*0.206)*9.8
    y1[3]=(float(y[1])*0.206)*9.8
    z1[3]=(float(z[1])*0.206)*9.8
    
    
    ax=x1[0]*0.1+x1[1]*0.2+x1[2]*0.3+x1[3]*0.4
    ay=y1[0]*0.1+y1[1]*0.2+y1[2]*0.3+y1[3]*0.4
    az=z1[0]*0.1+z1[1]*0.2+z1[2]*0.3+z1[3]*0.4
    
    #print(ax,ay,az)
    sp_x+=(ax*0.03)
    sp_y+=(ay*0.03)
    sp_z+=(az*0.03)
    disp_x+=(sp_x*0.03)
    disp_y+=(sp_y*0.03)
    disp_z+=(sp_z*0.03)
    list_x.append(disp_x)
    list_y.append(disp_y)
    list_z.append(disp_z)

    i=i+1
    return(disp_x,disp_y,disp_z)


def init():
    particles1.set_data([],[])
    particles2.set_data([],[])
    particles3.set_data([],[])
    return (particles1, particles2 ,particles3,)


def animate(frames):
    #global ti
    #o_ti=ti
    ti=time.clock()-start
    #te=ti-o_ti
    #print(te)
    data=ser.readline()
    #print(data)
    #print("!")
    while ser.isOpen()==True:
        a=getdata()
        t=int(ti*10)
        particles1.set_data(t,a[0])
        particles2.set_data(t,a[1])
        particles3.set_data(t,a[2])
        #particles1.text(a[0])
        return (particles1 ,particles2 ,particles3,)

anim=animation.FuncAnimation(fig, animate, init_func=init,
                             frames=200, interval=10, blit=True)

plt.show()
