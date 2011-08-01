﻿/*
 * Copyright (C) 2011  pleoNeX
 *
 *   This program is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.
 *
 *   You should have received a copy of the GNU General Public License
 *   along with this program.  If not, see <http://www.gnu.org/licenses/>. 
 *
 * Programador: pleoNeX
 * Programa utilizado: Microsoft Visual C# 2010 Express
 * Fecha: 18/02/2011
 * 
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PluginInterface;

namespace Images
{
    public partial class iNCGR : UserControl
    {
        NCLR paleta;
        NCGR tile;
        int startTile;
        IPluginHost pluginHost;
        string tVen;

        string oldDepth;
        int oldTiles;

        public iNCGR()
        {
            InitializeComponent();
            LeerIdioma();
        }
        public iNCGR(IPluginHost pluginHost,NCGR tile, NCLR paleta)
        {
            this.pluginHost = pluginHost;
            InitializeComponent();
            LeerIdioma();

            this.paleta = paleta;
            this.tile = tile;
            pic.Image = pluginHost.Bitmap_NCGR(tile, paleta, 0);
            this.numericWidth.Value = pic.Image.Width;
            this.numericHeight.Value = pic.Image.Height;
            this.comboDepth.Text = (tile.rahc.depth == ColorDepth.Depth4Bit ? "4 bpp" : "8 bpp");
            oldDepth = comboDepth.Text;
            switch (tile.orden)
            {
                case Orden_Tiles.No_Tiles:
                    oldTiles = 0;
                    comboBox1.SelectedIndex = 0;
                    break;
                case Orden_Tiles.Horizontal:
                    oldTiles = 1;
                    comboBox1.SelectedIndex = 1;
                    break;
            }
            this.comboDepth.SelectedIndexChanged += new EventHandler(comboDepth_SelectedIndexChanged);
            this.numericWidth.ValueChanged += new EventHandler(numericSize_ValueChanged);
            this.numericHeight.ValueChanged += new EventHandler(numericSize_ValueChanged);
            this.numericStart.ValueChanged += new EventHandler(numericStart_ValueChanged);

            Info();
        }

        void numericStart_ValueChanged(object sender, EventArgs e)
        {
            startTile = (int)numericStart.Value;
            pic.Image = pluginHost.Bitmap_NCGR(tile, paleta, startTile);
        }

        private void numericSize_ValueChanged(object sender, EventArgs e)
        {
            Actualizar_Imagen();
        }
        private void comboDepth_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboDepth.Text == oldDepth)
                return;

            oldDepth = comboDepth.Text;
            tile.rahc.depth = (comboDepth.Text == "4 bpp" ? ColorDepth.Depth4Bit : ColorDepth.Depth8Bit);

            if (comboDepth.Text == "4 bpp")
            {
                byte[] temp = pluginHost.Bit8ToBit4(pluginHost.TilesToBytes(tile.rahc.tileData.tiles));
                tile.rahc.tileData.tiles = pluginHost.BytesToTiles(temp);
            }
            else
            {
                byte[] temp = pluginHost.Bit4ToBit8(pluginHost.TilesToBytes(tile.rahc.tileData.tiles));
                tile.rahc.tileData.tiles = pluginHost.BytesToTiles(temp);
            }

            Actualizar_Imagen();
        }
        private void Actualizar_Imagen()
        {
            if (tile.orden != Orden_Tiles.No_Tiles)
            {
                tile.rahc.nTilesX = (ushort)(numericWidth.Value / 8);
                tile.rahc.nTilesY = (ushort)(numericHeight.Value / 8);
            }
            else
            {
                tile.rahc.nTilesX = (ushort)numericWidth.Value;
                tile.rahc.nTilesY = (ushort)numericHeight.Value;
            }

            pic.Image = pluginHost.Bitmap_NCGR(tile, paleta, startTile);
            pluginHost.Set_NCGR(tile);
        }
        private void Info()
        {
            listInfo.Items[0].SubItems.Add("0x" + String.Format("{0:X}", tile.cabecera.constant));
            listInfo.Items[1].SubItems.Add(tile.cabecera.nSection.ToString());
            listInfo.Items[2].SubItems.Add(new String(tile.rahc.id));
            listInfo.Items[3].SubItems.Add("0x" + String.Format("{0:X}", tile.rahc.size_section));
            listInfo.Items[4].SubItems.Add(tile.rahc.nTilesY.ToString() + " (0x" + String.Format("{0:X}", tile.rahc.nTilesY) + ')');
            listInfo.Items[5].SubItems.Add(tile.rahc.nTilesX.ToString() + " (0x" + String.Format("{0:X}", tile.rahc.nTilesX) + ')');
            listInfo.Items[6].SubItems.Add(Enum.GetName(tile.rahc.depth.GetType(), tile.rahc.depth));
            listInfo.Items[7].SubItems.Add("0x" + String.Format("{0:X}", tile.rahc.unknown1));
            listInfo.Items[8].SubItems.Add("0x" + String.Format("{0:X}", tile.rahc.tiledFlag));
            listInfo.Items[9].SubItems.Add("0x" + String.Format("{0:X}", tile.rahc.size_tiledata));
            listInfo.Items[10].SubItems.Add("0x" + String.Format("{0:X}", tile.rahc.unknown3));
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog o = new SaveFileDialog();
            o.AddExtension = true;
            o.DefaultExt = "bmp";
            o.Filter = "BitMaP (*.bmp)|*.bmp";
            o.OverwritePrompt = true;
            if (o.ShowDialog() == DialogResult.OK)
                pic.Image.Save(o.FileName);
            o.Dispose();
        }
        private void pic_DoubleClick(object sender, EventArgs e)
        {
            Form ven = new Form();
            PictureBox pcBox = new PictureBox();
            pcBox.Image = pic.Image;
            pcBox.SizeMode = PictureBoxSizeMode.AutoSize;

            ven.Controls.Add(pcBox);
            ven.BackColor = SystemColors.GradientInactiveCaption;
            ven.Text = tVen; ;
            ven.AutoScroll = true;
            ven.MaximumSize = new Size(1024, 768);
            ven.ShowIcon = false;
            ven.AutoSize = true;
            ven.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            ven.MaximizeBox = false;
            ven.Show();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (oldTiles == comboBox1.SelectedIndex)
                return;

            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    tile.orden = Orden_Tiles.No_Tiles;
                    tile.rahc.tileData.tiles[0] = pluginHost.TilesToBytes(tile.rahc.tileData.tiles);
                    break;
                case 1:
                    tile.orden = Orden_Tiles.Horizontal;
                    tile.rahc.tileData.tiles = pluginHost.BytesToTiles(tile.rahc.tileData.tiles[0]);
                    break;
                case 2:
                    tile.orden = Orden_Tiles.Vertical;
                    break;
            }
            oldTiles = comboBox1.SelectedIndex;

            Actualizar_Imagen();
        }

        private void LeerIdioma()
        {
            System.Xml.Linq.XElement xml = System.Xml.Linq.XElement.Load(Application.StartupPath + "\\Plugins\\ImagesLang.xml");
            xml = xml.Element(pluginHost.Get_Language()).Element("NCGR");

            label5.Text = xml.Element("S01").Value;
            groupProp.Text = xml.Element("S02").Value;
            columnPos.Text = xml.Element("S03").Value;
            columnCampo.Text = xml.Element("S04").Value;
            columnValor.Text = xml.Element("S05").Value;
            listInfo.Items[0].SubItems[1].Text = xml.Element("S06").Value;
            listInfo.Items[1].SubItems[1].Text = xml.Element("S07").Value;
            listInfo.Items[2].SubItems[1].Text = xml.Element("S08").Value;
            listInfo.Items[3].SubItems[1].Text = xml.Element("S09").Value;
            listInfo.Items[4].SubItems[1].Text = xml.Element("S0A").Value;
            listInfo.Items[5].SubItems[1].Text = xml.Element("S0B").Value;
            listInfo.Items[6].SubItems[1].Text = xml.Element("S0C").Value;
            listInfo.Items[7].SubItems[1].Text = xml.Element("S0D").Value;
            listInfo.Items[8].SubItems[1].Text = xml.Element("S0E").Value;
            listInfo.Items[9].SubItems[1].Text = xml.Element("S0F").Value;
            listInfo.Items[10].SubItems[1].Text = xml.Element("S10").Value;
            label3.Text = xml.Element("S11").Value;
            label1.Text = xml.Element("S12").Value;
            label2.Text = xml.Element("S13").Value;
            label6.Text = xml.Element("S14").Value;
            btnSave.Text = xml.Element("S15").Value;
            comboBox1.Items[0] = xml.Element("S16").Value;
            comboBox1.Items[1] = xml.Element("S17").Value;
            tVen = xml.Element("S19").Value;
            lblZoom.Text = xml.Element("S1A").Value;
            btnBgd.Text = xml.Element("S1B").Value;
            btnBgdTrans.Text = xml.Element("S1C").Value;
            checkTrans.Text = xml.Element("S1D").Value;
        }

        private void trackZoom_Scroll(object sender, EventArgs e)
        {
            Actualizar_Imagen(); // Devolvemos la imagen original para no perder calidad

            float scale = trackZoom.Value / 100f;
            Bitmap imagen = new Bitmap((int)(pic.Image.Width * scale), (int)(pic.Image.Height * scale));
            Graphics graficos = Graphics.FromImage(imagen);
            graficos.DrawImage(pic.Image, 0, 0, pic.Image.Width * scale, pic.Image.Height * scale);
            pic.Image = imagen;
            graficos.Dispose();
        }
        private void checkTrans_CheckedChanged(object sender, EventArgs e)
        {
            if (checkTrans.Checked)
            {
                Bitmap imagen = (Bitmap)pic.Image;
                imagen.MakeTransparent(paleta.pltt.paletas[tile.rahc.tileData.nPaleta[0]].colores[0]);
                pic.Image = imagen;
            }
            else
                Actualizar_Imagen();
        }
        private void btnBgd_Click(object sender, EventArgs e)
        {
            ColorDialog o = new ColorDialog();
            o.AllowFullOpen = true;
            o.AnyColor = true;

            if (o.ShowDialog() == DialogResult.OK)
            {
                pictureBgd.BackColor = o.Color;
                pic.BackColor = o.Color;
                btnBgdTrans.Enabled = true;
            }
        }
        private void btnBgdTrans_Click(object sender, EventArgs e)
        {
            btnBgdTrans.Enabled = false;

            pictureBgd.BackColor = Color.Transparent;
            pic.BackColor = Color.Transparent;
        }
    }
}
