version=`git rev-parse --short HEAD`
tmp_path=/tmp/NodeGraphProcessor-${version}

package_path=Assets/NodeGraphProcessor/
sample1_path=Assets/Examples/

mkdir -p $tmp_path

# Copy package and samples
cp -Rf $package_path $tmp_path
cp -Rf $sample1_path $tmp_path

# Override pathes
package_path=$tmp_path"/NodeGraphProcessor"
sample1_path=$tmp_path"/Examples"

# Go to the release branch
git checkout upm

rm -rf '*'

cp -Rf $package_path .

sample_path='Samples~'
mkdir $sample_path
mkdir $sample_path/"Examples"

cp -Rf $sample1_path $sample_path"/Examples"

git add -A
git commit -m "Publish version ${version}"

