#!/usr/bin/env python
# coding: utf-8

import numpy as np
import pandas as pd
import scipy.stats as st
import matplotlib.pyplot as plt

get_ipython().run_line_magic('matplotlib', 'notebook')

eps = np.log(3)

def noise(value, e, S):
    var = float(S)/float(e)
    n = np.random.laplace(0,var)
    return value + n

def interval(queries, conf=0.95):
    return st.t.interval(conf, len(queries)-1, loc=np.mean(queries), scale=st.sem(queries))

def simulation(ground_truth, n=10, eps=np.log(3), S=1):
    queries = [noise(ground_truth, eps, S)]
    results = []
    for i in range(2,n+1):
        queries.append(noise(ground_truth, eps, S))
        m = np.mean(queries)
        result = {
            "i": i,
            "mean": m,
            "interval": interval(queries),
            "within_one": (ground_truth-m < (S/2)) and (ground_truth-m > -(S/2))
        }
        results.append(result)
        
    return results, queries

def gen_bars(simulations):
    x = [i['i'] for i in simulations]
    y = [i['mean'] for i in simulations]
    e = ([abs(i['interval'][0]-i['mean']) for i in simulations], [abs(i['interval'][1]-i['mean']) for i in simulations])

    return (x,y,e)

def generate_graph(sims, title, truth, S):
    e1 = []
    e2 = []
    for e in sims:
        if e['within_one']:
            e1.append(e)
        else:
            e2.append(e)

    bars1 = gen_bars(e1)
    bars2 = gen_bars(e2)
    beingsaved = plt.figure(figsize=(10,6))
    plt.xscale('log')
    plt.xlabel('Number of Queries')
    plt.ylabel('Estimates')
    plt.axhline(y=truth, color='b', linestyle='-', label="ground truth")
    plt.errorbar(bars1[0], bars1[1], yerr=bars1[2], fmt='o', c='red', label=f"Estimate within {S}")
    plt.errorbar(bars2[0], bars2[1], yerr=bars2[2], fmt='o', c='green', label=f"Estimate outside {S}")
    plt.legend()
    plt
    beingsaved.savefig(title+'.eps',format='eps', dpi=1000, bbox_inches='tight')


sims = simulation(3, 200, 0.4, 1)
generate_graph(sims,'./plots/3-200-0.5-1')

sims = simulation(3, 200, 0.7, 1)
generate_graph(sims,'./plots/3-200-0.7-1')

sims = simulation(3, 200, 2, 1)
generate_graph(sims,'./plots/3-200-2-1')

sims = simulation(100, 200, np.log(3), 25)
ests = sims[0]
generate_graph(ests,'./plots/test1', 100, 25)

sims = simulation(100, 200, np.log(3), 1)
ests = sims[0]
generate_graph(ests,'./plots/query2', 100, 1)