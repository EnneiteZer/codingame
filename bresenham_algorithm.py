import sys
import math
from collections import namedtuple
import functools

def log(text):
    print(text, file=sys.stderr, flush=True)

def get_bresenham_line(x1, y1, x2, y2) -> list:
    return get_bresenham_line_by_symetry(x1, y1, x2, y2)

def get_bresenham_line_1octant(x1 , y1, x2, y2):
    dx = abs(x2 - x1)
    dy = y2 - y1
    line = []
    
    slope_coeff = 0
    if dx != 0: 
        slope_coeff = dy / abs(x2 - x1)

    error = 0
    y = y1
    for x in range(x1, x2 + 1):
        line.append((x, y))        
        error += slope_coeff
        if error > 0.5:
            y += 1
            error += -1
    return line

def get_bresenham_line_by_symetry(x1 , y1, x2, y2) -> list:
    line = []
    dx = x2 - x1
    dy = y2 - y1
    
    #log("BRESENHAM")
    if y1 > y2:
        #log("5th - 8th octants")
        line =  get_bresenham_line_by_symetry(x2, y2, x1, y1)    #exchange cells 
    else:
        if x1 <= x2:
            #1 octant
            if dx >= dy:
                #log("1st octant")
                line =  get_bresenham_line_1octant(x1, y1, x2, y2)
            #2th octant
            else:
                log("2nd octant")
                #line =  get_bresenham_line_1octant(x1, y1, y2, x2)
                line =  get_bresenham_line_1octant(x1, y1, x1 + y2 - y1, y1 + x2 - x1)
                line = [(x1 + y - y1, y1 + x - x1) for (x, y) in line]
        else:
            #log("3th - 4th octant")  
            line = get_bresenham_line_by_symetry(x1, y1, x2 + 2 * (x1 - x2), y2)
            line = list(zip([x - 2 * (x - x1) for (x, y) in line], [y  for (x, y) in line]))
    
    return line

# (3,2) (8,3) => [(3, 2), (4, 2), (5, 2), (6, 3), (7, 3), (8, 3)]
#print(get_bresenham_line(x1=3, y1=2, x2=8, y2=3))  
# (3,2) (6,6) => [(3, 2), (4, 3), (4, 4), (5, 5), (6, 6)]
#print(get_bresenham_line(x1=3, y1=2, x2=6, y2=6))
# (3,2) (2,6) => [(3, 2), (3, 3), (3, 4), (2, 5), (2, 6)]
#print(get_bresenham_line(x1=3, y1=2, x2=2, y2=6))
# (0,1) (0,6) => [(0, 1), (0, 2), (0, 3), (0, 4), (0, 5), (0, 6)]
print(get_bresenham_line(x1=0, y1=1, x2=0, y2=6))