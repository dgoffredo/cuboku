
template = '''
<Rectangle Tap="nut_Tap" 
           Opacity="{{StaticResource tappadOpacity}}" 
           p:Name="tappad_{x}_{y}_{z}" 
           Hold="cell_Hold" 
           HorizontalAlignment="Left" 
           Height="{{StaticResource tappadHeight}}" 
           Width="{{StaticResource tappadWidth}}"  
           VerticalAlignment="Top" 
           Margin="{margins}" 
           Fill="Bisque">
    <Rectangle.Resources>
        <System:Int32 x:Key="HomeX">{x}</System:Int32>
        <System:Int32 x:Key="HomeY">{y}</System:Int32>
        <System:Int32 x:Key="HomeZ">{z}</System:Int32>
    </Rectangle.Resources>
</Rectangle>'''

margins = {}

margins[(0,0,0)] = '40,402,0,0'
margins[(1,0,0)] = '165,402,0,0'
margins[(2,0,0)] = '290,402,0,0'
margins[(0,0,1)] = '83,357,0,0'
margins[(1,0,1)] = '206,357,0,0'
margins[(2,0,1)] = '329,357,0,0'
margins[(0,0,2)] = '118,312,0,0'
margins[(1,0,2)] = '241,312,0,0'
margins[(2,0,2)] = '364,312,0,0'
margins[(0,1,0)] = '40,260,0,0'
margins[(1,1,0)] = '165,260,0,0'
margins[(2,1,0)] = '290,260,0,0'
margins[(0,1,1)] = '83,215,0,0'
margins[(1,1,1)] = '206,215,0,0'
margins[(2,1,1)] = '329,215,0,0'
margins[(0,1,2)] = '118,170,0,0'
margins[(1,1,2)] = '241,170,0,0'
margins[(2,1,2)] = '364,170,0,0'
margins[(0,2,0)] = '40,115,0,0'
margins[(1,2,0)] = '165,115,0,0'
margins[(2,2,0)] = '290,115,0,0'
margins[(0,2,1)] = '83,70,0,0'
margins[(1,2,1)] = '206,70,0,0'
margins[(2,2,1)] = '329,70,0,0'
margins[(0,2,2)] = '118,25,0,0'
margins[(1,2,2)] = '241,25,0,0'
margins[(2,2,2)] = '364,25,0,0'
                
n = 3
for x in range(n):
    for y in range(n):
        for z in range(n):
            print('\n<!-- TapPad {x},{y},{z} -->'.format(x=x, y=y, z=z))
            print(template.format(x=x, y=y, z=z, margins=margins[(x, y, z)]))
