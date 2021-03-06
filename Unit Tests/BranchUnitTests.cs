﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Unit_Tests
{
    [TestClass]
    public class BranchUnitTests
    {
        public sealed class ViewModel : ViewModelBase
        {
            public bool UseB
            {
                get { return Properties.Get(false); }
                set { Properties.Set(value); }
            }

            public int A
            {
                get { return Properties.Get(7); }
                set { Properties.Set(value); }
            }

            public int B
            {
                get { return Properties.Get(11); }
                set { Properties.Set(value); }
            }

            public int CalculatedValue
            {
                get
                {
                    return Properties.Calculated(() =>
                    {
                        ++CalculatedValueExecutionCount;
                        return UseB ? B : A;
                    });
                }
            }

            public int CalculatedValueExecutionCount;
        }
        
        [TestMethod]
        public void Calculated_InitialValueIsCalculated()
        {
            var vm = new ViewModel();
            Assert.AreEqual(7, vm.CalculatedValue);
            Assert.AreEqual(1, vm.CalculatedValueExecutionCount);
        }

        [TestMethod]
        public void IndependentPropertyChanges_DoesNotRaisePropertyChangedForCalculated()
        {
            var changes = new List<string>();
            var vm = new ViewModel();
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);
            var value = vm.B;
            value = vm.CalculatedValue;
            vm.B = 13;
            CollectionAssert.AreEquivalent(new[] { "B" }, changes);
        }

        [TestMethod]
        public void DependentPropertyChanges_RaisesPropertyChangedForCalculated()
        {
            var changes = new List<string>();
            var vm = new ViewModel();
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);
            var value = vm.A;
            value = vm.CalculatedValue;
            vm.A = 13;
            CollectionAssert.AreEquivalent(new[] { "A", "CalculatedValue" }, changes);
        }

        [TestMethod]
        public void Calculated_AfterBranchSwitch_IsRecalculated()
        {
            var vm = new ViewModel();
            var value = vm.CalculatedValue;
            vm.UseB = true;
            Assert.AreEqual(11, vm.CalculatedValue);
            Assert.AreEqual(2, vm.CalculatedValueExecutionCount);
        }

        [TestMethod]
        public void IndependentPropertyChanges_AfterBranchSwitch_DoesNotRaisePropertyChangedForCalculated()
        {
            var changes = new List<string>();
            var vm = new ViewModel();
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);
            var value = vm.CalculatedValue;
            vm.UseB = true;
            value = vm.CalculatedValue;

            vm.A = 13;

            CollectionAssert.AreEquivalent(new[] { "UseB", "CalculatedValue", "A" }, changes);
        }

        [TestMethod]
        public void DependentPropertyChanges_AfterBranchSwitch_RaisesPropertyChangedForCalculated()
        {
            var changes = new List<string>();
            var vm = new ViewModel();
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);
            var value = vm.CalculatedValue;
            vm.UseB = true;
            value = vm.CalculatedValue;

            vm.B = 13;

            CollectionAssert.AreEquivalent(new[] { "UseB", "CalculatedValue", "B", "CalculatedValue" }, changes);
        }
    }
}
